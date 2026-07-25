namespace UniGame.StaticEcs.Features
{
    using UniGame.StaticEcs.Unity;

    /// <summary>Creates exact-interface Main-world target index resources.</summary>
    public static class KdTreeTargetIndex
    {
        /// <summary>Creates the Main-world KD-tree implementation.</summary>
        public static KdTreeTargetIndex<Main> Create() => new();
    }
}
