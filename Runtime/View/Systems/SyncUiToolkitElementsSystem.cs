namespace UniGame.StaticEcs.Features
{
    using System;
    using System.Collections.Generic;
    using FFS.Libraries.StaticEcs;
    using UnityEngine;
    using UnityEngine.UIElements;

    /// <summary>Creates and releases one Static ECS entity for each ECS-enabled UXML child.</summary>
    public class SyncUiToolkitElementsSystem<TWorld> : ISystem
        where TWorld : struct, IWorldType
    {
        private readonly List<ViewKey> _removed = new();
        private readonly List<UiToolkitElementRegistryResource<TWorld>.Entry> _newRoots = new();
        private readonly HashSet<EcsUiToolkitElement> _changed = new();
        private readonly HashSet<EcsUiToolkitElement> _failed = new();
        private readonly HashSet<EcsUiToolkitElement> _pending = new();
        private readonly List<EcsUiToolkitElement> _orderedPending = new();

        public void Init() => EcsUiToolkitElement.HierarchyChanged += OnHierarchyChanged;

        public void Update()
        {
            var registry = World<TWorld>.GetResource<UiToolkitElementRegistryResource<TWorld>>();
            _removed.Clear();
            _newRoots.Clear();
            _pending.Clear();
            _pending.UnionWith(_changed);
            _changed.Clear();

            foreach (var pair in registry.Entries)
            {
                var entry = pair.Value;
                var active = entry.View != null &&
                             entry.ViewEntity.TryUnpack<TWorld>(out _);
                var root = active ? entry.View.Root : null;
                if (!ReferenceEquals(entry.Root, root) || !active)
                {
                    ReleaseChildren(entry);
                    entry.Root = root;
                    if (!active)
                        _removed.Add(pair.Key);
                    else if (root != null)
                        _newRoots.Add(entry);
                    continue;
                }

                if (_pending.Count == 0)
                    continue;

                for (var i = entry.Children.Count - 1; i >= 0; i--)
                {
                    var child = entry.Children[i];
                    if (_pending.Contains(child.Element) &&
                        (!child.Element.CanRegisterEntity || !IsInside(root, child.Element)))
                        ReleaseChild(entry, i);
                }
            }

            foreach (var key in _removed)
                registry.Entries.Remove(key);

            foreach (var entry in _newRoots)
            {
                var children = entry.Root.Query<EcsUiToolkitElement>().ToList();
                children.Sort((left, right) => Depth(left).CompareTo(Depth(right)));
                foreach (var element in children)
                    if (element.CanRegisterEntity)
                        RegisterChild(entry, element);
            }

            _orderedPending.Clear();
            _orderedPending.AddRange(_pending);
            _orderedPending.Sort((left, right) => Depth(left).CompareTo(Depth(right)));
            foreach (var element in _orderedPending)
            {
                if (!element.CanRegisterEntity)
                    continue;

                foreach (var entry in registry.Entries.Values)
                {
                    if (entry.Root == null || !IsInside(entry.Root, element))
                        continue;

                    RegisterChild(entry, element);
                    break;
                }
            }

            _failed.RemoveWhere(element =>
            {
                if (!element.CanRegisterEntity)
                    return true;
                foreach (var entry in registry.Entries.Values)
                    if (entry.Root != null && IsInside(entry.Root, element))
                        return false;
                return true;
            });
        }

        public void Destroy()
        {
            EcsUiToolkitElement.HierarchyChanged -= OnHierarchyChanged;
            _changed.Clear();
            _failed.Clear();
            _pending.Clear();
            _orderedPending.Clear();
            if (!World<TWorld>.HasResource<UiToolkitElementRegistryResource<TWorld>>())
                return;

            var registry = World<TWorld>.GetResource<UiToolkitElementRegistryResource<TWorld>>();
            foreach (var entry in registry.Entries.Values)
                ReleaseChildren(entry);
            registry.Entries.Clear();
        }

        private void OnHierarchyChanged(EcsUiToolkitElement element)
        {
            _failed.Remove(element);
            _changed.Add(element);
        }

        private static bool IsInside(VisualElement root, VisualElement element)
        {
            for (var current = element; current != null; current = current.parent)
                if (ReferenceEquals(current, root))
                    return true;

            return false;
        }

        private static int Depth(VisualElement element)
        {
            var depth = 0;
            for (var parent = element.parent; parent != null; parent = parent.parent)
                depth++;
            return depth;
        }

        private void RegisterChild(UiToolkitElementRegistryResource<TWorld>.Entry entry,
            EcsUiToolkitElement element)
        {
            foreach (var child in entry.Children)
                if (ReferenceEquals(child.Element, element))
                    return;

            var entity = World<TWorld>.NewEntity<Default>();
            var appliedConverters = 0;
            try
            {
                entity.Set(new UiToolkitElementComponent { Element = element });
                element.AddTypeComponent<TWorld>(entity);
                entity.Set(new World<TWorld>.Link<UiToolkitElementParent>(entry.ViewEntity));
                for (var index = 0; index < element.Converters.Count; index++)
                {
                    var converter = element.Converters[index];
                    if (converter == null || !converter.isEnabled)
                        continue;
                    appliedConverters = index + 1;
                    converter.Apply<TWorld>(entity);
                }

                element.Entity = entity.GID;
                entry.Children.Add(new UiToolkitElementRegistryResource<TWorld>.Child
                {
                    Element = element,
                    Entity = entity.GID
                });
                _failed.Remove(element);
            }
            catch (Exception error)
            {
                for (var index = appliedConverters - 1; index >= 0; index--)
                {
                    var converter = element.Converters[index];
                    if (converter == null || !converter.isEnabled ||
                        converter is not IUiToolkitElementDestroyHandler handler)
                        continue;
                    try
                    {
                        handler.OnEntityDestroy<TWorld>(entity);
                    }
                    catch (Exception cleanupError)
                    {
                        Debug.LogException(cleanupError);
                    }
                }

                if (element.Entity == entity.GID)
                    element.Entity = default;
                entity.Destroy();
                _changed.Add(element);
                if (_failed.Add(element))
                    Debug.LogException(new InvalidOperationException(
                        $"Failed to register UI Toolkit element {element.GetType().Name}.", error));
            }
        }

        private static void ReleaseChildren(UiToolkitElementRegistryResource<TWorld>.Entry entry)
        {
            for (var i = entry.Children.Count - 1; i >= 0; i--)
                ReleaseChild(entry, i);
        }

        private static void ReleaseChild(UiToolkitElementRegistryResource<TWorld>.Entry entry,
            int index)
        {
            var child = entry.Children[index];
            if (child.Element != null && child.Element.Entity == child.Entity)
                child.Element.Entity = default;
            if (child.Entity.TryUnpack<TWorld>(out var entity))
            {
                if (child.Element != null)
                {
                    foreach (var converter in child.Element.Converters)
                        if (converter is { isEnabled: true } &&
                            converter is IUiToolkitElementDestroyHandler handler)
                            try
                            {
                                handler.OnEntityDestroy<TWorld>(entity);
                            }
                            catch (Exception cleanupError)
                            {
                                Debug.LogException(cleanupError);
                            }
                }
                entity.Destroy();
            }

            entry.Children.RemoveAt(index);
        }
    }
}
