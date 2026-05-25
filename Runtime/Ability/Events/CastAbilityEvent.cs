using FFS.Libraries.StaticEcs;

namespace unigame.staticecs.features {
    /// <summary>
    /// Request to start casting an ability. Consumed by <c>AbilityCastSystem</c>, which validates
    /// the roster, cooldown, and concurrency, then attaches <see cref="AbilityCastComponent"/>.
    /// </summary>
    public struct CastAbilityEvent : IEvent {
        public EntityGID Caster;
        public AbilityId AbilityId;
        public EntityGID Target;
    }
}
