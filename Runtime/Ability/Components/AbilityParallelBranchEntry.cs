using FFS.Libraries.StaticEcs;

namespace UniGame.StaticEcs.Features {
    public struct AbilityParallelBranchEntry : IMultiComponent {
        public EntityGID BranchCast;
        public StepStatus Status;
        public bool Completed;
    }
}
