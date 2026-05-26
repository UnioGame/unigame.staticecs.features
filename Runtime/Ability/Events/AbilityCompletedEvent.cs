using FFS.Libraries.StaticEcs;

namespace unigame.staticecs.features {
    /// <summary>
    /// Emitted by the progression system at the moment a cast-entity is destroyed. Mirrors the
    /// previous AbilityStateChangedEvent(Completed/Cancelled) without the phase-enum baggage.
    /// </summary>
    public struct AbilityCompletedEvent : IEvent {
        public EntityGID Caster;
        public AbilityId AbilityId;
        public EntityGID CastEntity;
        public AbilityCompletedReason Reason;
    }
}
