namespace UniGame.StaticEcs.Features
{
    using FFS.Libraries.StaticEcs;

    /// <summary>
    /// Terminal status of the leaf that just completed on a cast-entity. Written together with
    /// <see cref="AbilityStepReadyTag"/>; consumed and cleared by
    /// <c>AbilityStepProgressionSystem</c>. Default-zero <see cref="StepStatus.Running"/> is
    /// never observed — the progression system always sees Success or Failed.
    /// </summary>
    public struct AbilityStepStatusComponent : IComponent
    {
        /// <summary>The terminal status produced by the completed step.</summary>
        public StepStatus Status;
    }
}
