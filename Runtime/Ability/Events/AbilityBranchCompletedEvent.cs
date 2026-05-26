using FFS.Libraries.StaticEcs;

namespace unigame.staticecs.features {
    public struct AbilityBranchCompletedEvent : IEvent {
        public EntityGID ParentCast;
        public EntityGID BranchCast;
        public AbilityId AbilityId;
        public StepStatus Status;
    }
}
