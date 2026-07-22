namespace UniGame.StaticEcs.Features
{
    using FFS.Libraries.StaticEcs;

    /// <summary>
    /// Request to start casting an ability. Consumed by <c>AbilityCastSystem</c>, which
    /// re-validates ability-internal invariants (roster + concurrency, see plan §1b) and
    /// spawns a cast-entity carrying <see cref="AbilityCastComponent"/>.
    /// </summary>
    public struct CastAbilityEvent : IEvent
    {
        public EntityGID Caster;
        public AbilityId AbilityId;
        public EntityGID Target;
    }
}
