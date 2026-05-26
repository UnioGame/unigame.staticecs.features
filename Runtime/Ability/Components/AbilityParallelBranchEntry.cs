using FFS.Libraries.StaticEcs;

namespace unigame.staticecs.features {
    public struct AbilityParallelBranchEntry : IMultiComponent {
        public EntityGID BranchCast;
        public StepStatus Status;
        public bool Completed;
    }
}
