using FFS.Libraries.StaticEcs;

namespace unigame.staticecs.features {
    /// <summary>
    /// Drives the asynchronous part of a Wait leaf. Lives on the cast-entity (one Wait at a
    /// time per cast; Parallel branches use separate branch cast-entities).
    /// Drained by <c>AbilityWaitSystem</c>; on expiry the system writes
    /// <see cref="StepStatus.Success"/> + <c>AbilityStepReadyTag</c> and removes itself.
    /// </summary>
    public struct AbilityWaitState : IComponent {
        public float TimeLeft;
    }
}
