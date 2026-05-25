using System;
using System.Collections.Generic;
using FFS.Libraries.StaticEcs;

namespace unigame.staticecs.features {
    /// <summary>
    /// World-scoped resource that maps <see cref="AbilityId"/> to its <see cref="AbilityDefinition"/>
    /// and a cast handler. Two registration overloads are exposed: a lightweight
    /// <see cref="AbilityCastDelegate"/> for one-off scripts and a full
    /// <see cref="IAbilityHandler{TWorld}"/> for stateful or DI-resolved handlers.
    /// </summary>
    public sealed class AbilityRegistry<TWorld> : IResource
        where TWorld : struct, IWorldType {
        private readonly Dictionary<int, AbilityDefinition> _defs = new();
        private readonly Dictionary<int, IAbilityHandler<TWorld>> _handlers = new();
        private readonly Dictionary<int, AbilityCastDelegate> _delegates = new();

        public int Count => _defs.Count;

        public void Register(AbilityDefinition definition, IAbilityHandler<TWorld> handler) {
            if (definition == null) throw new ArgumentNullException(nameof(definition));
            if (handler == null) throw new ArgumentNullException(nameof(handler));

            _defs[definition.Id.Value] = definition;
            _handlers[definition.Id.Value] = handler;
            _delegates.Remove(definition.Id.Value);
        }

        public void Register(AbilityDefinition definition, AbilityCastDelegate onCast) {
            if (definition == null) throw new ArgumentNullException(nameof(definition));
            if (onCast == null) throw new ArgumentNullException(nameof(onCast));

            _defs[definition.Id.Value] = definition;
            _delegates[definition.Id.Value] = onCast;
            _handlers.Remove(definition.Id.Value);
        }

        public bool Unregister(AbilityId id) {
            var key = id.Value;
            var removed = _defs.Remove(key);
            _handlers.Remove(key);
            _delegates.Remove(key);
            return removed;
        }

        public bool Contains(AbilityId id) => _defs.ContainsKey(id.Value);

        public bool TryGet(AbilityId id, out AbilityDefinition definition) {
            return _defs.TryGetValue(id.Value, out definition);
        }

        public AbilityDefinition Get(AbilityId id) {
            if (!_defs.TryGetValue(id.Value, out var def)) {
                throw new InvalidOperationException($"AbilityRegistry<{typeof(TWorld).Name}>: ability {id} is not registered.");
            }
            return def;
        }

        public bool Invoke(AbilityId id, EntityGID caster, ReadOnlySpan<EntityGID> targets) {
            var key = id.Value;
            if (_handlers.TryGetValue(key, out var handler)) {
                handler.OnCast(caster, targets);
                return true;
            }
            if (_delegates.TryGetValue(key, out var del)) {
                del(caster, targets);
                return true;
            }
            return false;
        }
    }
}
