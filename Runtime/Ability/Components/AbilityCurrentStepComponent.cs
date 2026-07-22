namespace UniGame.StaticEcs.Features
{
    using FFS.Libraries.StaticEcs;

    /// <summary>
    /// Pointer to the leaf step config currently active on the cast-entity. Holds a managed
    /// reference; the underlying object lives in <see cref="AbilityRegistry{TWorld}"/> for the
    /// lifetime of the world, so no GC pressure is introduced per cast. Absent between
    /// activations (e.g. while the progression system is searching for the next leaf).
    /// </summary>
    public struct AbilityCurrentStepComponent : IComponent
    {
        /// <summary>The configuration of the currently active leaf step.</summary>
        public IAbilityStepConfig Config;
    }
}
