namespace UniGame.StaticEcs.Features
{
    using FFS.Libraries.StaticEcs;

    public struct AbilityBranchCompletedEvent : IEvent
    {
        public EntityGID ParentCast;
        public EntityGID BranchCast;
        public AbilityId AbilityId;
        public StepStatus Status;
    }
}
