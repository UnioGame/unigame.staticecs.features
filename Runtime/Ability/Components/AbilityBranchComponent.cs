namespace UniGame.StaticEcs.Features
{
    using FFS.Libraries.StaticEcs;

    /// <summary>Tracks one branch cast created by a parallel ability step.</summary>
    public struct AbilityBranchComponent : IMultiComponent
    {
        /// <summary>The branch cast entity.</summary>
        public EntityGID BranchCast;

        /// <summary>The latest branch result.</summary>
        public StepStatus Status;

        /// <summary>Whether the branch has reached a terminal result.</summary>
        public bool Completed;
    }
}
