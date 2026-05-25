using FFS.Libraries.StaticEcs;

namespace unigame.staticecs.features {
    /// <summary>
    /// Lifecycle notification covering Started, PhaseAdvanced, Completed, Cancelled and
    /// Interrupted transitions. Consumers (UI, FX) read <see cref="Reason"/> to branch.
    /// </summary>
    public struct AbilityStateChangedEvent : IEvent {
        public EntityGID Caster;
        public AbilityId AbilityId;
        public AbilityPhase Phase;
        public AbilityChangeReason Reason;
    }
}
