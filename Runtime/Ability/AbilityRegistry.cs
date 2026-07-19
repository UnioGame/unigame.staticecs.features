using System;
using System.Collections.Generic;
using FFS.Libraries.StaticEcs;

namespace UniGame.StaticEcs.Features {
    /// <summary>
    /// World-scoped resource pairing each <see cref="AbilityId"/> with its
    /// <see cref="AbilityDefinition"/> and the root <see cref="IAbilityStepConfig"/> of its
    /// step pipeline. Replaces the legacy handler/delegate dictionary — runtime behaviour now
    /// lives in the step graph; the registry holds only configuration.
    /// </summary>
    public sealed class AbilityRegistry<TWorld> : IResource
        where TWorld : struct, IWorldType {
        private readonly Dictionary<int, Entry> _entries = new();

        public int Count => _entries.Count;

        public void Register(AbilityDefinition definition, IAbilityStepConfig root) {
            if (definition == null) throw new ArgumentNullException(nameof(definition));
            if (root == null) throw new ArgumentNullException(nameof(root));

            _entries[definition.Id.Value] = new Entry(definition, root);
        }

        public bool Unregister(AbilityId id) {
            return _entries.Remove(id.Value);
        }

        public bool Contains(AbilityId id) => _entries.ContainsKey(id.Value);

        public bool TryGet(AbilityId id, out AbilityDefinition definition, out IAbilityStepConfig root) {
            if (_entries.TryGetValue(id.Value, out var entry)) {
                definition = entry.Definition;
                root = entry.Root;
                return true;
            }

            definition = null;
            root = null;
            return false;
        }

        public AbilityDefinition GetDefinition(AbilityId id) {
            if (_entries.TryGetValue(id.Value, out var entry)) {
                return entry.Definition;
            }
            throw new InvalidOperationException(
                $"AbilityRegistry<{typeof(TWorld).Name}>: ability {id} is not registered.");
        }

        public IAbilityStepConfig GetRoot(AbilityId id) {
            if (_entries.TryGetValue(id.Value, out var entry)) {
                return entry.Root;
            }
            throw new InvalidOperationException(
                $"AbilityRegistry<{typeof(TWorld).Name}>: ability {id} is not registered.");
        }

        private readonly struct Entry {
            public readonly AbilityDefinition Definition;
            public readonly IAbilityStepConfig Root;

            public Entry(AbilityDefinition definition, IAbilityStepConfig root) {
                Definition = definition;
                Root = root;
            }
        }
    }
}
