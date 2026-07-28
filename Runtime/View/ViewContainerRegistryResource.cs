namespace UniGame.StaticEcs.Features
{
    using System;
    using System.Collections.Generic;
    using FFS.Libraries.StaticEcs;
    using UnityEngine;

    /// <summary>Stores named View System parent containers and their occupancy.</summary>
    public class ViewContainerRegistryResource<TWorld> : IResource
        where TWorld : struct, IWorldType
    {
        private readonly Dictionary<string, Entry> _entries =
            new(StringComparer.Ordinal);

        /// <summary>Registers or replaces one named container.</summary>
        public void Register(string name, Transform parent, int capacity = 1)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Container name must be specified.", nameof(name));

            if (parent == null)
                throw new ArgumentNullException(nameof(parent));

            _entries[name] = new Entry
            {
                Parent = parent,
                Capacity = Math.Max(1, capacity)
            };
        }

        /// <summary>Removes a named container when no view occupies it.</summary>
        public bool Unregister(string name)
        {
            return _entries.TryGetValue(name, out var entry) &&
                   entry.Occupants.Count == 0 &&
                   _entries.Remove(name);
        }

        internal bool TryReserve(
            string name,
            ViewKey key,
            bool useBusyContainer,
            out Transform parent)
        {
            parent = null;
            if (!_entries.TryGetValue(name, out var entry) || entry.Parent == null)
                return false;

            if (entry.Occupants.Count >= entry.Capacity && !useBusyContainer)
                return false;

            entry.Occupants.Add(key);
            parent = entry.Parent;
            return true;
        }

        internal void Release(string name, ViewKey key)
        {
            if (_entries.TryGetValue(name, out var entry))
                entry.Occupants.Remove(key);
        }

        private sealed class Entry
        {
            public Transform Parent;
            public int Capacity;
            public readonly HashSet<ViewKey> Occupants = new();
        }
    }
}
