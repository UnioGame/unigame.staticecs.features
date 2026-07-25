namespace UniGame.StaticEcs.Features
{
    using UniGame.StaticEcs.Unity;

    /// <summary>Creates exact-type Main-world ability step activator resources.</summary>
    public static class AbilityStepActivatorRegistry
    {
        /// <summary>Creates the Main-world activator registry.</summary>
        public static AbilityStepActivatorRegistry<Main> Create() => new();
    }
}
