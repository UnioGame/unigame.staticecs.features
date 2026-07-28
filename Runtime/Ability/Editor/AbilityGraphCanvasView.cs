namespace UniGame.StaticEcs.Features.Editor.AbilityGraph
{
    using System;
    using System.Collections.Generic;
    using UnityEditor;
    using UnityEditor.Experimental.GraphView;
    using UnityEngine;
    using UnityEngine.UIElements;

    internal sealed class AbilityGraphCanvasView : GraphView
    {
        private AbilityAsset _asset;
        private AbilityGraphProjection _projection = AbilityGraphProjection.Empty;
        private readonly Action<AbilityGraphProjection.Node> _nodeSelected;
        private readonly Action _requestRefresh;
        private readonly List<Edge> _edges = new();
        private readonly List<AbilityGraphNodeView> _nodes = new();

        public AbilityGraphCanvasView(
            Action<AbilityGraphProjection.Node> nodeSelected,
            Action requestRefresh
        )
        {
            _nodeSelected = nodeSelected;
            _requestRefresh = requestRefresh;

            style.flexGrow = 1f;
            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());
            SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);

            var grid = new GridBackground();
            Insert(0, grid);
            grid.StretchToParentSize();

            var minimap = new MiniMap { anchored = true };
            minimap.SetPosition(new Rect(12f, 12f, 180f, 120f));
            Add(minimap);

            RegisterCallback<MouseDownEvent>(OnBackgroundMouseDown);
            RegisterCallback<KeyDownEvent>(OnKeyDown);
            graphViewChanged += OnGraphViewChanged;
        }

        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
        {
            var result = new List<Port>();
            foreach (var port in ports)
            {
                if (port == startPort)
                    continue;
                if (port.node == startPort.node)
                    continue;
                if (port.direction == startPort.direction)
                    continue;
                result.Add(port);
            }
            return result;
        }

        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            var edgeView = FindAncestor<Edge>(evt.target as VisualElement);
            if (edgeView != null)
            {
                AbilityGraphProjection.Edge? edgeData = edgeView.userData
                    is AbilityGraphProjection.Edge edge
                    ? edge
                    : null;
                AbilityGraphContextCommandRegistry.AppendCommands(
                    evt.menu,
                    new AbilityGraphContextCommandContext
                    {
                        Asset = _asset,
                        CanvasView = this,
                        EdgeView = edgeView,
                        Edge = edgeData,
                        Node = edgeData.HasValue ? FindNodeById(edgeData.Value.ToId) : null,
                        Target = AbilityGraphContextTarget.Edge,
                    }
                );
                return;
            }

            AbilityGraphContextCommandRegistry.AppendCommands(
                evt.menu,
                new AbilityGraphContextCommandContext
                {
                    Asset = _asset,
                    CanvasView = this,
                    Target = AbilityGraphContextTarget.Background,
                }
            );
        }

        public void Render(AbilityAsset asset, AbilityGraphProjection projection)
        {
            _asset = asset;
            _projection = projection ?? AbilityGraphProjection.Empty;
            ClearGraph();

            if (_projection.Nodes.Count == 0)
            {
                _nodeSelected?.Invoke(null);
                return;
            }

            var rowsByDepth = new Dictionary<int, int>();
            var nodesById = new Dictionary<string, AbilityGraphNodeView>(StringComparer.Ordinal);

            for (var i = 0; i < _projection.Nodes.Count; i++)
            {
                var node = _projection.Nodes[i];
                rowsByDepth.TryGetValue(node.Depth, out var rowIndex);
                rowsByDepth[node.Depth] = rowIndex + 1;

                var position = new Rect(56f + node.Depth * 340f, 56f + rowIndex * 210f, 280f, 160f);
                var view = new AbilityGraphNodeView(
                    _asset,
                    this,
                    node,
                    position,
                    HandleNodeSelected
                );
                _nodes.Add(view);
                nodesById[node.Id] = view;
                AddElement(view);
            }

            for (var i = 0; i < _projection.Edges.Count; i++)
            {
                var edge = _projection.Edges[i];
                if (!nodesById.TryGetValue(edge.FromId, out var outputNode))
                    continue;

                if (!nodesById.TryGetValue(edge.ToId, out var inputNode))
                    continue;

                if (!outputNode.TryGetOutputPort(edge.Label, out var outputPort))
                    continue;

                if (inputNode.InputPort == null)
                    continue;

                var viewEdge = outputPort.ConnectTo(inputNode.InputPort);
                viewEdge.capabilities = Capabilities.Selectable;
                viewEdge.userData = edge;
                AddElement(viewEdge);
                _edges.Add(viewEdge);
            }
        }

        private void HandleNodeSelected(AbilityGraphProjection.Node node)
        {
            _nodeSelected?.Invoke(node);
        }

        private void OnKeyDown(KeyDownEvent evt)
        {
            if (evt.keyCode != KeyCode.Delete && evt.keyCode != KeyCode.Backspace)
                return;

            if (_asset == null || selection.Count == 0)
                return;

            DeleteSelectedElements();
            evt.StopPropagation();
        }

        private void DeleteSelectedElements()
        {
            var changed = false;

            for (var i = 0; i < selection.Count; i++)
            {
                if (selection[i] is AbilityGraphNodeView nodeView)
                {
                    if (
                        AbilityGraphAssetEditing.RemoveNodeReference(
                            _asset,
                            nodeView.ProjectionNode.Config
                        )
                    )
                        changed = true;
                }
                else if (
                    selection[i] is Edge edge
                    && edge.output?.node is AbilityGraphNodeView outputNode
                )
                    if (
                        AbilityGraphAssetEditing.DisconnectPort(
                            _asset,
                            outputNode.ProjectionNode.Config,
                            edge.output.portName
                        )
                    )
                        changed = true;
            }

            if (changed)
            {
                EditorApplication.delayCall -= new EditorApplication.CallbackFunction(
                    _requestRefresh
                );
                EditorApplication.delayCall += new EditorApplication.CallbackFunction(
                    _requestRefresh
                );
            }
        }

        private void OnBackgroundMouseDown(MouseDownEvent evt)
        {
            if (evt.button != 0)
                return;

            if (evt.target == this || evt.target is GridBackground)
            {
                ClearSelection();
                _nodeSelected?.Invoke(null);
            }
        }

        private void ClearGraph()
        {
            for (var i = 0; i < _edges.Count; i++)
            {
                RemoveElement(_edges[i]);
            }
            _edges.Clear();

            for (var i = 0; i < _nodes.Count; i++)
            {
                RemoveElement(_nodes[i]);
            }
            _nodes.Clear();
        }

        private static TElement FindAncestor<TElement>(VisualElement element)
            where TElement : VisualElement
        {
            while (element != null)
            {
                if (element is TElement match)
                    return match;

                element = element.parent;
            }

            return null;
        }

        private AbilityGraphProjection.Node FindNodeById(string nodeId)
        {
            if (string.IsNullOrWhiteSpace(nodeId) || _projection == null)
                return null;

            for (var i = 0; i < _projection.Nodes.Count; i++)
            {
                var node = _projection.Nodes[i];
                if (string.Equals(node.Id, nodeId, StringComparison.Ordinal))
                    return node;
            }

            return null;
        }

        private GraphViewChange OnGraphViewChanged(GraphViewChange change)
        {
            if (_asset == null)
                return change;

            var needsRefresh = false;

            if (change.edgesToCreate != null && change.edgesToCreate.Count > 0)
            {
                for (var i = 0; i < change.edgesToCreate.Count; i++)
                {
                    HandleEdgeAdded(change.edgesToCreate[i]);
                }

                needsRefresh = true;
            }

            if (change.elementsToRemove != null && change.elementsToRemove.Count > 0)
                for (var i = 0; i < change.elementsToRemove.Count; i++)
                {
                    if (change.elementsToRemove[i] is Edge removedEdge)
                    {
                        HandleEdgeRemoved(removedEdge);
                        needsRefresh = true;
                    }
                }

            if (needsRefresh && _requestRefresh != null)
            {
                EditorApplication.delayCall -= new EditorApplication.CallbackFunction(
                    _requestRefresh
                );
                EditorApplication.delayCall += new EditorApplication.CallbackFunction(
                    _requestRefresh
                );
            }

            return change;
        }

        private void HandleEdgeAdded(Edge edge)
        {
            if (edge.output?.node is not AbilityGraphNodeView outputNodeView)
                return;

            if (edge.input?.node is not AbilityGraphNodeView inputNodeView)
                return;

            AbilityGraphAssetEditing.ConnectPort(
                _asset,
                outputNodeView.ProjectionNode.Config,
                edge.output.portName,
                inputNodeView.ProjectionNode.Config
            );
        }

        private void HandleEdgeRemoved(Edge edge)
        {
            if (edge.output?.node is not AbilityGraphNodeView outputNodeView)
                return;

            AbilityGraphAssetEditing.DisconnectPort(
                _asset,
                outputNodeView.ProjectionNode.Config,
                edge.output.portName
            );
        }
    }
}
