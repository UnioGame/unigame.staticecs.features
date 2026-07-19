using FFS.Libraries.StaticEcs;

namespace UniGame.StaticEcs.Features {
    public struct AbilityBranchCompletedEvent : IEvent {
        public EntityGID ParentCast;
        public EntityGID BranchCast;
        public AbilityId AbilityId;
        public StepStatus Status;
    }
}
