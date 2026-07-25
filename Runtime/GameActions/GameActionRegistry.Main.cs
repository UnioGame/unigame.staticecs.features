namespace UniGame.StaticEcs.Features
{
    using UniGame.StaticEcs.Unity;

    /// <summary>Creates exact-type Main-world game action registry resources.</summary>
    public static class GameActionRegistry
    {
        /// <summary>Creates the Main-world registry.</summary>
        public static GameActionRegistry<Main> Create() => new();
    }
}
