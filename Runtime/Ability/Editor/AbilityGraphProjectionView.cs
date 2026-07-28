namespace UniGame.StaticEcs.Features.Editor.AbilityGraph
{
    using UnityEngine;
    using UnityEngine.UIElements;

    internal sealed class AbilityGraphProjectionView : ScrollView
    {
        public AbilityGraphProjectionView()
        {
            style.flexGrow = 1f;
            horizontalScrollerVisibility = ScrollerVisibility.Hidden;
            verticalScrollerVisibility = ScrollerVisibility.Auto;
        }

        public void Render(AbilityGraphProjection projection)
        {
            contentContainer.Clear();

            if (projection == null || projection.Nodes.Count == 0)
            {
                Add(
                    new HelpBox(
                        "No graph data available for the selected ability.",
                        HelpBoxMessageType.Info
                    )
                );
                return;
            }

            if (projection.Warnings.Count > 0)
                for (var i = 0; i < projection.Warnings.Count; i++)
                {
                    var warning = new HelpBox(projection.Warnings[i], HelpBoxMessageType.Warning);
                    warning.style.marginBottom = 6f;
                    Add(warning);
                }

            for (var i = 0; i < projection.Nodes.Count; i++)
            {
                Add(CreateNodeRow(projection.Nodes[i]));
            }
        }

        private static VisualElement CreateNodeRow(AbilityGraphProjection.Node node)
        {
            var row = new VisualElement();
            row.style.flexDirection = FlexDirection.Row;
            row.style.alignItems = Align.FlexStart;
            row.style.marginBottom = 8f;

            var indent = new VisualElement();
            indent.style.width = node.Depth * 28f;
            indent.style.flexShrink = 0f;
            row.Add(indent);

            var edgeLabel = new Label(
                string.IsNullOrEmpty(node.ParentEdgeLabel) ? "Root" : node.ParentEdgeLabel
            );
            edgeLabel.style.minWidth = 76f;
            edgeLabel.style.marginTop = 8f;
            edgeLabel.style.unityTextAlign = TextAnchor.MiddleLeft;
            edgeLabel.style.color = new Color(0.60f, 0.72f, 0.88f, 1f);
            row.Add(edgeLabel);

            var card = new VisualElement();
            card.style.flexGrow = 1f;
            card.style.backgroundColor = new Color(0.13f, 0.13f, 0.15f, 1f);
            card.style.borderTopLeftRadius = 6f;
            card.style.borderTopRightRadius = 6f;
            card.style.borderBottomLeftRadius = 6f;
            card.style.borderBottomRightRadius = 6f;
            card.style.borderLeftWidth = 1f;
            card.style.borderRightWidth = 1f;
            card.style.borderTopWidth = 1f;
            card.style.borderBottomWidth = 1f;
            card.style.borderLeftColor = new Color(0.24f, 0.24f, 0.28f, 1f);
            card.style.borderRightColor = new Color(0.24f, 0.24f, 0.28f, 1f);
            card.style.borderTopColor = new Color(0.24f, 0.24f, 0.28f, 1f);
            card.style.borderBottomColor = new Color(0.24f, 0.24f, 0.28f, 1f);
            card.style.paddingLeft = 10f;
            card.style.paddingRight = 10f;
            card.style.paddingTop = 8f;
            card.style.paddingBottom = 8f;

            var title = new Label(node.Title);
            title.style.unityFontStyleAndWeight = FontStyle.Bold;
            title.style.fontSize = 13;
            card.Add(title);

            var subtitle = new Label(node.Subtitle);
            subtitle.style.marginTop = 2f;
            subtitle.style.color = new Color(0.78f, 0.78f, 0.82f, 1f);
            card.Add(subtitle);

            var metadata = new Label(
                $"Kind: {node.Config.Kind}   NodeGuid: {(string.IsNullOrWhiteSpace(node.Config.NodeGuid) ? "<empty>" : node.Config.NodeGuid)}"
            );
            metadata.style.marginTop = 4f;
            metadata.style.fontSize = 11;
            metadata.style.color = new Color(0.55f, 0.55f, 0.60f, 1f);
            card.Add(metadata);

            row.Add(card);
            return row;
        }
    }
}
