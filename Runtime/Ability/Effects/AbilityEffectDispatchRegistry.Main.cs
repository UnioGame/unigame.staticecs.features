namespace UniGame.StaticEcs.Features
{
    using UniGame.StaticEcs.Unity;

    /// <summary>Creates exact-type Main-world ability effect dispatcher resources.</summary>
    public static class AbilityEffectDispatchRegistry
    {
        /// <summary>Creates the Main-world dispatcher registry.</summary>
        public static AbilityEffectDispatchRegistry<Main> Create() => new();
    }
}
