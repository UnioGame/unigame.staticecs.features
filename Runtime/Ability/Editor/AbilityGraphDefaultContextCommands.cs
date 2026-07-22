namespace UniGame.StaticEcs.Features.Editor.AbilityGraph
{
    using UnityEditor;
    using UnityEngine;

    internal static class AbilityGraphDefaultContextCommands
    {
        [AbilityGraphContextCommand(
            AbilityGraphContextTarget.Background,
            "Graph/Add Node...",
            Order = -10
        )]
        private static void AddNode(AbilityGraphContextCommandContext context)
        {
            if (context.Asset == null)
            {
                return;
            }

            if (context.Asset.Root != null)
            {
                EditorUtility.DisplayDialog(
                    "Graph Already Has a Root Node",
                    "Right-click on a Sequence, Parallel, Conditional, or Repeat node to add child nodes to it.",
                    "OK"
                );
                return;
            }

            AbilityGraphNodePickerWindow.Open(entry => CreateRootNode(context.Asset, entry));
        }

        [AbilityGraphContextCommand(
            AbilityGraphContextTarget.Node,
            "Sequence/Add Child...",
            Order = -10,
            NodeType = typeof(SequenceStepConfig)
        )]
        private static void AddSequenceChild(AbilityGraphContextCommandContext context)
        {
            if (context.Asset == null || context.NodeConfig is not SequenceStepConfig sequence)
            {
                return;
            }

            AbilityGraphNodePickerWindow.Open(entry =>
            {
                if (
                    AbilityGraphAssetEditing.AppendChild(
                        context.Asset,
                        sequence,
                        "_children",
                        entry
                    )
                )
                {
                    AbilityGraphEditorWindow.OpenTab(context.Asset);
                    Selection.activeObject = context.Asset;
                }
            });
        }

        [AbilityGraphContextCommand(
            AbilityGraphContextTarget.Node,
            "Parallel/Add Branch...",
            Order = -10,
            NodeType = typeof(ParallelStepConfig)
        )]
        private static void AddParallelBranch(AbilityGraphContextCommandContext context)
        {
            if (context.Asset == null || context.NodeConfig is not ParallelStepConfig parallel)
            {
                return;
            }

            AbilityGraphNodePickerWindow.Open(entry =>
            {
                if (
                    AbilityGraphAssetEditing.AppendChild(
                        context.Asset,
                        parallel,
                        "_children",
                        entry
                    )
                )
                {
                    AbilityGraphEditorWindow.OpenTab(context.Asset);
                    Selection.activeObject = context.Asset;
                }
            });
        }

        [AbilityGraphContextCommand(
            AbilityGraphContextTarget.Node,
            "Conditional/Set True Branch...",
            Order = -10,
            NodeType = typeof(ConditionalStepConfig)
        )]
        private static void SetConditionalTrueBranch(AbilityGraphContextCommandContext context)
        {
            if (
                context.Asset == null
                || context.NodeConfig is not ConditionalStepConfig conditional
            )
            {
                return;
            }

            AbilityGraphNodePickerWindow.Open(entry =>
            {
                if (conditional.IfTrue != null)
                {
                    var replace = EditorUtility.DisplayDialog(
                        "Replace True Branch",
                        "True branch is already assigned. Replace it?",
                        "Replace",
                        "Cancel"
                    );
                    if (!replace)
                    {
                        return;
                    }
                }

                if (
                    AbilityGraphAssetEditing.AssignChild(
                        context.Asset,
                        conditional,
                        "_ifTrue",
                        entry
                    )
                )
                {
                    AbilityGraphEditorWindow.OpenTab(context.Asset);
                    Selection.activeObject = context.Asset;
                }
            });
        }

        [AbilityGraphContextCommand(
            AbilityGraphContextTarget.Node,
            "Conditional/Set False Branch...",
            Order = 0,
            NodeType = typeof(ConditionalStepConfig)
        )]
        private static void SetConditionalFalseBranch(AbilityGraphContextCommandContext context)
        {
            if (
                context.Asset == null
                || context.NodeConfig is not ConditionalStepConfig conditional
            )
            {
                return;
            }

            AbilityGraphNodePickerWindow.Open(entry =>
            {
                if (conditional.IfFalse != null)
                {
                    var replace = EditorUtility.DisplayDialog(
                        "Replace False Branch",
                        "False branch is already assigned. Replace it?",
                        "Replace",
                        "Cancel"
                    );
                    if (!replace)
                    {
                        return;
                    }
                }

                if (
                    AbilityGraphAssetEditing.AssignChild(
                        context.Asset,
                        conditional,
                        "_ifFalse",
                        entry
                    )
                )
                {
                    AbilityGraphEditorWindow.OpenTab(context.Asset);
                    Selection.activeObject = context.Asset;
                }
            });
        }

        [AbilityGraphContextCommand(
            AbilityGraphContextTarget.Node,
            "Repeat/Set Body...",
            Order = -10,
            NodeType = typeof(RepeatStepConfig)
        )]
        private static void SetRepeatBody(AbilityGraphContextCommandContext context)
        {
            if (context.Asset == null || context.NodeConfig is not RepeatStepConfig repeat)
            {
                return;
            }

            AbilityGraphNodePickerWindow.Open(entry =>
            {
                if (repeat.Body != null)
                {
                    var replace = EditorUtility.DisplayDialog(
                        "Replace Repeat Body",
                        "Repeat body is already assigned. Replace it?",
                        "Replace",
                        "Cancel"
                    );
                    if (!replace)
                    {
                        return;
                    }
                }

                if (AbilityGraphAssetEditing.AssignChild(context.Asset, repeat, "_body", entry))
                {
                    AbilityGraphEditorWindow.OpenTab(context.Asset);
                    Selection.activeObject = context.Asset;
                }
            });
        }

        [AbilityGraphContextCommand(
            AbilityGraphContextTarget.Background,
            "Graph/Open Project Browser",
            Order = 0
        )]
        private static void OpenProjectBrowser(AbilityGraphContextCommandContext context)
        {
            AbilityGraphBrowserWindow.OpenProjectBrowser();
        }

        [AbilityGraphContextCommand(
            AbilityGraphContextTarget.Background,
            "Graph/Open Runtime Browser",
            Order = 10
        )]
        private static void OpenRuntimeBrowser(AbilityGraphContextCommandContext context)
        {
            AbilityGraphBrowserWindow.OpenRuntimeBrowser();
        }

        [AbilityGraphContextCommand(
            AbilityGraphContextTarget.Background,
            "Graph/Clear Selection",
            Order = 20
        )]
        [AbilityGraphContextCommand(
            AbilityGraphContextTarget.Node,
            "Node/Clear Selection",
            Order = 20
        )]
        [AbilityGraphContextCommand(
            AbilityGraphContextTarget.Edge,
            "Edge/Clear Selection",
            Order = 20
        )]
        private static void ClearSelection(AbilityGraphContextCommandContext context)
        {
            context.CanvasView?.ClearSelection();
        }

        [AbilityGraphContextCommand(
            AbilityGraphContextTarget.Node,
            "Node/Copy NodeGuid",
            Order = 0
        )]
        private static void CopyNodeGuid(AbilityGraphContextCommandContext context)
        {
            EditorGUIUtility.systemCopyBuffer = context.NodeConfig?.NodeGuid ?? string.Empty;
        }

        [AbilityGraphContextCommand(AbilityGraphContextTarget.Node, "Node/Delete", Order = 15)]
        private static void DeleteNode(AbilityGraphContextCommandContext context)
        {
            if (context.Asset == null || context.NodeConfig == null)
            {
                return;
            }

            var isRoot = ReferenceEquals(context.Asset.Root, context.NodeConfig);
            var confirmed = EditorUtility.DisplayDialog(
                isRoot ? "Delete Root Node" : "Delete Node",
                isRoot
                    ? "Delete the root node and clear the graph root?"
                    : "Delete the selected node and remove its incoming connection?",
                "Delete",
                "Cancel"
            );
            if (!confirmed)
            {
                return;
            }

            if (!AbilityGraphAssetEditing.RemoveNodeReference(context.Asset, context.NodeConfig))
            {
                return;
            }

            AbilityGraphEditorWindow.OpenTab(context.Asset);
            Selection.activeObject = context.Asset;
        }

        [AbilityGraphContextCommand(
            AbilityGraphContextTarget.Node,
            "Node/Copy Node Type",
            Order = 10
        )]
        private static void CopyNodeType(AbilityGraphContextCommandContext context)
        {
            EditorGUIUtility.systemCopyBuffer =
                context.NodeConfig?.GetType().FullName ?? string.Empty;
        }

        [AbilityGraphContextCommand(AbilityGraphContextTarget.Edge, "Edge/Disconnect", Order = 0)]
        private static void DisconnectEdge(AbilityGraphContextCommandContext context)
        {
            if (context.Asset == null || context.NodeConfig == null)
            {
                return;
            }

            var confirmed = EditorUtility.DisplayDialog(
                "Disconnect Edge",
                "Remove the connection represented by this edge?",
                "Disconnect",
                "Cancel"
            );
            if (!confirmed)
            {
                return;
            }

            if (!AbilityGraphAssetEditing.RemoveNodeReference(context.Asset, context.NodeConfig))
            {
                return;
            }

            AbilityGraphEditorWindow.OpenTab(context.Asset);
            Selection.activeObject = context.Asset;
        }

        [AbilityGraphContextCommand(
            AbilityGraphContextTarget.Node,
            "Node/Ping Ability Asset",
            Order = 30
        )]
        [AbilityGraphContextCommand(
            AbilityGraphContextTarget.Edge,
            "Edge/Ping Ability Asset",
            Order = 30
        )]
        private static void PingAbilityAsset(AbilityGraphContextCommandContext context)
        {
            if (context.Asset == null)
            {
                return;
            }

            EditorGUIUtility.PingObject(context.Asset);
            Selection.activeObject = context.Asset;
        }

        [AbilityGraphContextCommand(
            AbilityGraphContextTarget.Node,
            "Composite/Copy Child Count",
            Order = 40,
            NodeType = typeof(SequenceStepConfig)
        )]
        [AbilityGraphContextCommand(
            AbilityGraphContextTarget.Node,
            "Composite/Copy Child Count",
            Order = 40,
            NodeType = typeof(ParallelStepConfig)
        )]
        private static void CopyCompositeChildCount(AbilityGraphContextCommandContext context)
        {
            var value = context.NodeConfig switch
            {
                SequenceStepConfig sequence => sequence.ChildCount.ToString(),
                ParallelStepConfig parallel => parallel.ChildCount.ToString(),
                _ => string.Empty,
            };

            EditorGUIUtility.systemCopyBuffer = value;
        }

        private static void CreateRootNode(
            AbilityAsset asset,
            AbilityGraphNodeTypeRegistry.Entry entry
        )
        {
            if (asset == null || entry?.Type == null)
            {
                return;
            }

            if (!AbilityGraphAssetEditing.ReplaceRoot(asset, entry))
            {
                return;
            }

            AbilityGraphEditorWindow.OpenTab(asset);
            Selection.activeObject = asset;
        }
    }
}
