using System;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace UniGame.StaticEcs.Features.Editor.AbilityGraph {
    internal sealed class AbilityGraphNodeView : Node {
        private readonly AbilityAsset _asset;
        private readonly AbilityGraphCanvasView _canvasView;
        private readonly Action<AbilityGraphProjection.Node> _nodeSelected;
        private readonly Dictionary<string, Port> _outputPorts = new(StringComparer.Ordinal);

        public AbilityGraphNodeView(
            AbilityAsset asset,
            AbilityGraphCanvasView canvasView,
            AbilityGraphProjection.Node node,
            Rect position,
            Action<AbilityGraphProjection.Node> nodeSelected) {
            _asset = asset;
            _canvasView = canvasView;
            ProjectionNode = node;
            _nodeSelected = nodeSelected;

            title = node.Title;
            capabilities = Capabilities.Selectable | Capabilities.Movable | Capabilities.Ascendable;

            viewDataKey = node.Id;
            SetPosition(position);

            var accent = ResolveAccent(node.Config.Kind);
            titleContainer.style.backgroundColor = accent;
            titleContainer.style.minHeight = 28f;

            if (!string.IsNullOrWhiteSpace(node.Subtitle)) {
                var subtitle = new Label(node.Subtitle);
                subtitle.style.marginBottom = 4f;
                subtitle.style.whiteSpace = WhiteSpace.Normal;
                extensionContainer.Add(subtitle);
            }

            var metadata = new Label($"{node.Config.GetType().Name}\nNodeGuid: {FormatNodeGuid(node.Config.NodeGuid)}");
            metadata.style.whiteSpace = WhiteSpace.Normal;
            metadata.style.color = new Color(0.72f, 0.72f, 0.78f, 1f);
            metadata.style.fontSize = 11f;
            extensionContainer.Add(metadata);

            if (!string.IsNullOrEmpty(node.ParentId)) {
                InputPort = CreatePort(Direction.Input, string.IsNullOrEmpty(node.ParentEdgeLabel) ? "In" : node.ParentEdgeLabel, Port.Capacity.Single);
                inputContainer.Add(InputPort);
            }

            AddOutputPorts(node.Config);

            RefreshExpandedState();
            RefreshPorts();

            RegisterCallback<MouseDownEvent>(OnNodeMouseDown);
        }

        public AbilityGraphProjection.Node ProjectionNode { get; }
        public Port InputPort { get; }

        public bool TryGetOutputPort(string label, out Port port) {
            return _outputPorts.TryGetValue(label ?? string.Empty, out port);
        }

        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt) {
            AbilityGraphContextCommandRegistry.AppendCommands(evt.menu, new AbilityGraphContextCommandContext {
                Asset = _asset,
                CanvasView = _canvasView,
                NodeView = this,
                Node = ProjectionNode,
                Target = AbilityGraphContextTarget.Node,
            });
        }

        private void AddOutputPorts(IAbilityStepConfig config) {
            switch (config) {
                case SequenceStepConfig sequence:
                    for (var i = 0; i < sequence.ChildCount; i++) {
                        AddOutputPort($"Step {i + 1}");
                    }
                    break;

                case ParallelStepConfig parallel:
                    for (var i = 0; i < parallel.ChildCount; i++) {
                        AddOutputPort($"Branch {i + 1}");
                    }
                    break;

                case ConditionalStepConfig:
                    AddOutputPort("True");
                    AddOutputPort("False");
                    break;

                case RepeatStepConfig:
                    AddOutputPort("Body");
                    break;
            }
        }

        private void AddOutputPort(string label) {
            var port = CreatePort(Direction.Output, label, Port.Capacity.Single);
            outputContainer.Add(port);
            _outputPorts[label] = port;
        }

        private Port CreatePort(Direction direction, string label, Port.Capacity capacity) {
            var port = InstantiatePort(Orientation.Horizontal, direction, capacity, typeof(IAbilityStepConfig));
            port.portName = label;
            port.portColor = new Color(0.62f, 0.76f, 0.95f, 1f);
            if (direction == Direction.Input) {
                port.edgeConnector?.activators.Clear();
            }
            return port;
        }

        private void OnNodeMouseDown(MouseDownEvent evt) {
            if (evt.button != 0) {
                return;
            }

            _nodeSelected?.Invoke(ProjectionNode);
        }

        private static string FormatNodeGuid(string value) {
            return string.IsNullOrWhiteSpace(value) ? "<empty>" : value;
        }

        private static Color ResolveAccent(AbilityStepKind kind) {
            return kind switch {
                AbilityStepKind.Sequence => new Color(0.19f, 0.31f, 0.53f, 1f),
                AbilityStepKind.Parallel => new Color(0.28f, 0.20f, 0.47f, 1f),
                AbilityStepKind.Conditional => new Color(0.23f, 0.42f, 0.31f, 1f),
                AbilityStepKind.Repeat => new Color(0.48f, 0.33f, 0.15f, 1f),
                AbilityStepKind.ApplyDamage => new Color(0.48f, 0.19f, 0.19f, 1f),
                AbilityStepKind.ApplyEffect => new Color(0.41f, 0.20f, 0.45f, 1f),
                AbilityStepKind.AoeQuery => new Color(0.20f, 0.38f, 0.43f, 1f),
                AbilityStepKind.SetPrimaryTargetFromAoe => new Color(0.29f, 0.36f, 0.20f, 1f),
                _ => new Color(0.24f, 0.24f, 0.28f, 1f),
            };
        }
    }
}