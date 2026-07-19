using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace UniGame.StaticEcs.Features.Editor.AbilityGraph {
    internal sealed class AbilityGraphProjection {
        internal readonly struct Edge {
            public readonly string FromId;
            public readonly string ToId;
            public readonly string Label;

            public Edge(string fromId, string toId, string label) {
                FromId = fromId;
                ToId = toId;
                Label = label;
            }
        }

        internal sealed class Node {
            public string Id;
            public string ParentId;
            public string ParentEdgeLabel;
            public int Depth;
            public IAbilityStepConfig Config;
            public string Title;
            public string Subtitle;
        }

        public static readonly AbilityGraphProjection Empty = new(Array.Empty<Node>(), Array.Empty<Edge>(), Array.Empty<string>());

        public AbilityGraphProjection(IReadOnlyList<Node> nodes, IReadOnlyList<Edge> edges, IReadOnlyList<string> warnings) {
            Nodes = nodes;
            Edges = edges;
            Warnings = warnings;
        }

        public IReadOnlyList<Node> Nodes { get; }
        public IReadOnlyList<Edge> Edges { get; }
        public IReadOnlyList<string> Warnings { get; }

        public static AbilityGraphProjection Build(AbilityAsset asset) {
            if (asset == null || asset.Root == null) {
                return Empty;
            }

            var nodes = new List<Node>();
            var edges = new List<Edge>();
            var warnings = new List<string>();
            var visited = new HashSet<IAbilityStepConfig>(ReferenceEqualityComparer<IAbilityStepConfig>.Instance);
            var guidUsage = new Dictionary<string, int>(StringComparer.Ordinal);

            Traverse(asset.Root, depth: 0, parentId: null, parentEdgeLabel: null, nodes, edges, warnings, visited, guidUsage);

            foreach (var entry in guidUsage) {
                if (entry.Value > 1 && !string.IsNullOrWhiteSpace(entry.Key)) {
                    warnings.Add($"Duplicate NodeGuid detected: {entry.Key}");
                }
            }

            return new AbilityGraphProjection(nodes, edges, warnings);
        }

        private static void Traverse(
            IAbilityStepConfig config,
            int depth,
            string parentId,
            string parentEdgeLabel,
            List<Node> nodes,
            List<Edge> edges,
            List<string> warnings,
            HashSet<IAbilityStepConfig> visited,
            Dictionary<string, int> guidUsage) {
            if (config == null) {
                return;
            }

            var nodeId = BuildNodeId(config, nodes.Count);
            if (!visited.Add(config)) {
                warnings.Add($"Cycle or repeated reference skipped at {nodeId}.");
                if (!string.IsNullOrEmpty(parentId)) {
                    edges.Add(new Edge(parentId, nodeId, parentEdgeLabel ?? "ref"));
                }
                return;
            }

            if (!string.IsNullOrWhiteSpace(config.NodeGuid)) {
                guidUsage.TryGetValue(config.NodeGuid, out var usageCount);
                guidUsage[config.NodeGuid] = usageCount + 1;
            }

            var node = new Node {
                Id = nodeId,
                ParentId = parentId,
                ParentEdgeLabel = parentEdgeLabel,
                Depth = depth,
                Config = config,
                Title = BuildTitle(config),
                Subtitle = BuildSubtitle(config),
            };

            nodes.Add(node);

            if (!string.IsNullOrEmpty(parentId)) {
                edges.Add(new Edge(parentId, nodeId, parentEdgeLabel ?? "child"));
            }

            switch (config) {
                case SequenceStepConfig sequence:
                    for (var i = 0; i < sequence.ChildCount; i++) {
                        Traverse(sequence.GetChild(i), depth + 1, nodeId, $"Step {i + 1}", nodes, edges, warnings, visited, guidUsage);
                    }
                    break;

                case ParallelStepConfig parallel:
                    for (var i = 0; i < parallel.ChildCount; i++) {
                        Traverse(parallel.GetChild(i), depth + 1, nodeId, $"Branch {i + 1}", nodes, edges, warnings, visited, guidUsage);
                    }
                    break;

                case ConditionalStepConfig conditional:
                    Traverse(conditional.IfTrue, depth + 1, nodeId, "True", nodes, edges, warnings, visited, guidUsage);
                    Traverse(conditional.IfFalse, depth + 1, nodeId, "False", nodes, edges, warnings, visited, guidUsage);
                    break;

                case RepeatStepConfig repeat:
                    Traverse(repeat.Body, depth + 1, nodeId, "Body", nodes, edges, warnings, visited, guidUsage);
                    break;
            }
        }

        private static string BuildNodeId(IAbilityStepConfig config, int index) {
            if (!string.IsNullOrWhiteSpace(config.NodeGuid)) {
                return config.NodeGuid;
            }

            return $"{config.Kind}:{index}";
        }

        private static string BuildTitle(IAbilityStepConfig config) {
            var customTitle = config.GetType().GetCustomAttributes(typeof(AbilityStepEditorAttribute), false);
            if (customTitle.Length > 0 && customTitle[0] is AbilityStepEditorAttribute metadata
                && !string.IsNullOrWhiteSpace(metadata.DisplayName)) {
                return metadata.DisplayName;
            }

            return config.Kind switch {
                AbilityStepKind.Wait => "Wait",
                AbilityStepKind.ApplyDamage => "Apply Damage",
                AbilityStepKind.ApplyEffect => "Apply Effect",
                AbilityStepKind.AoeQuery => "AoE Query",
                AbilityStepKind.SetPrimaryTargetFromAoe => "Select Primary Target",
                AbilityStepKind.Sequence => "Sequence",
                AbilityStepKind.Parallel => "Parallel",
                AbilityStepKind.Conditional => "Conditional",
                AbilityStepKind.Repeat => "Repeat",
                _ => ObjectNames.NicifyVariableName(config.Kind.ToString())
            };
        }

        private static string BuildSubtitle(IAbilityStepConfig config) {
            return config switch {
                WaitStepConfig wait => $"Duration {wait.Duration:0.##}s",
                ApplyDamageStepConfig damage => $"{damage.Type} {damage.Amount:0.##} -> {damage.Mode}",
                ApplyEffectStepConfig effect => $"Effect {effect.EffectId} -> {effect.Mode}",
                AoeQueryStepConfig aoe => $"Radius {aoe.Radius:0.##}, Max {aoe.MaxTargets}",
                SetPrimaryTargetFromAoeStepConfig selector => $"Selector {selector.Selector}",
                ParallelStepConfig parallel => $"{parallel.JoinPolicy}, CancelRemaining={parallel.CancelRemainingOnJoin}",
                ConditionalStepConfig conditional => $"Condition {conditional.Condition?.GetType().Name ?? "None"}",
                RepeatStepConfig repeat => $"MaxIterations {repeat.MaxIterations}",
                SequenceStepConfig sequence => $"Children {sequence.ChildCount}",
                _ => config.GetType().Name
            };
        }

        private sealed class ReferenceEqualityComparer<T> : IEqualityComparer<T> where T : class {
            public static readonly ReferenceEqualityComparer<T> Instance = new();

            public bool Equals(T x, T y) {
                return ReferenceEquals(x, y);
            }

            public int GetHashCode(T obj) {
                return System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(obj);
            }
        }
    }
}