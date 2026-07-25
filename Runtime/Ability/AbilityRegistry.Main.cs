namespace UniGame.StaticEcs.Features
{
    using UniGame.StaticEcs.Unity;

    /// <summary>Creates exact-type Main-world ability registry resources.</summary>
    public static class AbilityRegistry
    {
        /// <summary>Creates a registry stored as <see cref="AbilityRegistry{TWorld}"/> for Main.</summary>
        public static AbilityRegistry<Main> Create() => new();
    }
}
