namespace UniGame.StaticEcs.Features
{
    using FFS.Libraries.StaticEcs;

    /// <summary>Stores an inline root step for a cast that is not resolved through the registry.</summary>
    public struct AbilityRootComponent : IComponent
    {
        /// <summary>The inline root step configuration.</summary>
        public IAbilityStepConfig Root;
    }
}
