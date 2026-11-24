namespace UniGame.StaticEcs.Features
{
    using System.Collections.Generic;
    using FFS.Libraries.StaticEcs;
    using UniGame.UiSystem.Runtime.UiToolkit;
    using UnityEngine;
    using UnityEngine.UIElements;

    /// <summary>Tracks live UI Toolkit views until their element entities are released.</summary>
    public sealed class UiToolkitElementRegistryResource<TWorld> : IResource
        where TWorld : struct, IWorldType
    {
        internal readonly Dictionary<ViewKey, Entry> Entries = new();

        /// <summary>Finds the live UI Toolkit view associated with an ECS view key.</summary>
        public bool TryGetView(ViewKey key, out IUiToolkitView view)
        {
            view = null;
            if (!Entries.TryGetValue(key, out var entry) || entry.View == null)
                return false;

            view = entry.View;
            return true;
        }

        /// <summary>Finds the highest registered View panel with pickable content at a screen point.</summary>
        public bool TryGetTopPanelAt(Vector2 topLeftScreen, out IPanel topPanel)
        {
            topPanel = null;
            var bestOrder = float.NegativeInfinity;
            foreach (var entry in Entries.Values)
            {
                var root = entry.Root;
                var panel = root?.panel;
                if (entry.View == null || panel == null)
                    continue;

                var point = RuntimePanelUtils.ScreenToPanel(panel, topLeftScreen);
                var hit = panel.Pick(point);
                for (var current = hit; current != null; current = current.parent)
                {
                    if (!ReferenceEquals(current, root))
                        continue;
                    if (!TryGetPanelSortingOrder(panel, out var order))
                        break;
                    if (order >= bestOrder)
                    {
                        topPanel = panel;
                        bestOrder = order;
                    }
                    break;
                }
            }

            return topPanel != null;
        }

        /// <summary>Returns the live host order for a registered UI Toolkit panel.</summary>
        public bool TryGetPanelSortingOrder(IPanel panel, out float order)
        {
            foreach (var entry in Entries.Values)
            {
                if (entry.View == null || !ReferenceEquals(entry.Root?.panel, panel))
                    continue;
#if UNITY_6000_5_OR_NEWER
                var host = entry.View.PanelRenderer;
                if (host == null)
                    continue;
                order = host.sortingOrder;
#else
                var host = entry.View.UiDocument;
                if (host == null || host.panelSettings == null)
                    continue;
                order = host.panelSettings.sortingOrder;
#endif
                return true;
            }

            order = 0;
            return false;
        }

        internal void Register(ViewKey key, EntityGID viewEntity, IUiToolkitView view)
        {
            if (!Entries.TryGetValue(key, out var entry))
            {
                entry = new Entry();
                Entries.Add(key, entry);
            }

            entry.ViewEntity = viewEntity;
            entry.View = view;
        }

        internal void Unregister(ViewKey key)
        {
            if (Entries.TryGetValue(key, out var entry))
                entry.View = null;
        }

        internal sealed class Entry
        {
            public EntityGID ViewEntity;
            public IUiToolkitView View;
            public VisualElement Root;
            public readonly List<Child> Children = new();
        }

        internal struct Child
        {
            public EcsUiToolkitElement Element;
            public EntityGID Entity;
        }
    }
}
