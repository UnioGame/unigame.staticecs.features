using FFS.Libraries.StaticEcs;

namespace unigame.staticecs.features {
    /// <summary>
    /// Terminal status of the leaf that just completed on a cast-entity. Written together with
    /// <see cref="AbilityStepReadyTag"/>; consumed and cleared by
    /// <c>AbilityStepProgressionSystem</c>. Default-zero <see cref="StepStatus.Running"/> is
    /// never observed — the progression system always sees Success or Failed.
    /// </summary>
    public struct AbilityStepLastStatus : IComponent {
        public StepStatus Status;
    }
}
