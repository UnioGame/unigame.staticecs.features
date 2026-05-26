using System;
using System.Collections.Generic;
using FFS.Libraries.StaticEcs;

namespace unigame.staticecs.features {
    /// <summary>
    /// World-scoped resource mapping a concrete <see cref="IAbilityStepConfig"/> Type to its
    /// stateless activator singleton. Populated once by <see cref="AbilityFeature{TWorld}"/> at
    /// type-registration time and never mutated afterwards, so reads from
    /// <c>AbilityStepProgressionSystem</c> are allocation-free and thread-friendly.
    /// </summary>
    public sealed class AbilityStepActivatorRegistry<TWorld> : IResource
        where TWorld : struct, IWorldType {
        private readonly Dictionary<Type, IAbilityStepActivator<TWorld>> _activators = new();

        public void Register<TConfig>(IAbilityStepActivator<TWorld> activator)
            where TConfig : class, IAbilityStepConfig {
            if (activator == null) {
                throw new ArgumentNullException(nameof(activator));
            }
            _activators[typeof(TConfig)] = activator;
        }

        public bool TryResolve(Type configType, out IAbilityStepActivator<TWorld> activator) {
            return _activators.TryGetValue(configType, out activator);
        }

        public IAbilityStepActivator<TWorld> Resolve(Type configType) {
            if (_activators.TryGetValue(configType, out var activator)) {
                return activator;
            }
            throw new InvalidOperationException(
                $"AbilityStepActivatorRegistry<{typeof(TWorld).Name}>: no activator registered for {configType.Name}.");
        }

        public bool IsRegistered(Type configType) => _activators.ContainsKey(configType);
    }
}
