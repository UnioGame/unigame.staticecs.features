using FFS.Libraries.StaticEcs;

namespace unigame.staticecs.features {
    /// <summary>
    /// Emitted whenever the progression system retires the current leaf on a cast-entity.
    /// <see cref="FinalStatus"/> is always Success or Failed (Running is never observed here).
    /// </summary>
    public struct AbilityStepCompletedEvent : IEvent {
        public EntityGID CastEntity;
        public AbilityId AbilityId;
        public string NodeGuid;
        public AbilityStepKind Kind;
        public StepStatus FinalStatus;
    }
}
