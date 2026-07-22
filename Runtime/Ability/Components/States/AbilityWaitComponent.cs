namespace UniGame.StaticEcs.Features
{
    using FFS.Libraries.StaticEcs;

    /// <summary>
    /// Drives the asynchronous part of a Wait leaf. Lives on the cast-entity (one Wait at a
    /// time per cast; Parallel branches use separate branch cast-entities).
    /// Drained by <c>AbilityWaitSystem</c>; on expiry the system writes
    /// <see cref="StepStatus.Success"/> + <c>AbilityStepReadyTag</c> and removes itself.
    /// </summary>
    public struct AbilityWaitComponent : IComponent
    {
        /// <summary>The remaining wait duration in seconds.</summary>
        public float TimeLeft;
    }
}
