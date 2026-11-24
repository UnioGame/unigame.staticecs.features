namespace UniGame.StaticEcs.Features
{
    using FFS.Libraries.StaticEcs;
    using UnityEngine;
    using UnityEngine.UIElements;

    /// <summary>Owns only the temporary pointer visual; gameplay state remains in ECS.</summary>
    public sealed class UiToolkitDragVisualResource<TWorld> : IResource
        where TWorld : struct, IWorldType
    {
        private VisualElement _ghost;
        private float _width;
        private float _height;

        public void Show(VisualElement source, Vector2 screen, IPanel preferredPanel)
        {
            if (source?.panel == null)
            {
                Hide();
                return;
            }

            if (_ghost == null)
            {
                _width = source.worldBound.width;
                _height = source.worldBound.height;
                _ghost = new VisualElement { pickingMode = PickingMode.Ignore };
                _ghost.style.position = Position.Absolute;
                _ghost.style.width = _width;
                _ghost.style.height = _height;
                _ghost.style.backgroundColor = new Color(0.22f, 0.24f, 0.27f, 0.8f);
                _ghost.style.opacity = 0.85f;
                var preview = (source as IUiToolkitDragPreviewSource)?.CreateDragPreview();
                if (preview != null)
                {
                    DisablePicking(preview);
                    preview.style.flexGrow = 1;
                    _ghost.Add(preview);
                }
            }

            var panel = preferredPanel ?? source.panel;
            if (_ghost.parent != panel.visualTree)
            {
                _ghost.RemoveFromHierarchy();
                panel.visualTree.Add(_ghost);
            }

            var topLeftScreen = new Vector2(screen.x, Screen.height - screen.y);
            var panelPoint = RuntimePanelUtils.ScreenToPanel(panel, topLeftScreen);
            var local = panel.visualTree.WorldToLocal(panelPoint);
            _ghost.style.left = local.x - _width * 0.5f;
            _ghost.style.top = local.y - _height * 0.5f;
        }

        public void Hide()
        {
            _ghost?.RemoveFromHierarchy();
            _ghost = null;
            _width = 0;
            _height = 0;
        }

        private static void DisablePicking(VisualElement element)
        {
            element.pickingMode = PickingMode.Ignore;
            foreach (var child in element.Children())
                DisablePicking(child);
        }
    }
}
