namespace UniGame.StaticEcs.Features
{
    using FFS.Libraries.StaticEcs;

    /// <summary>Accumulates scaled ECS time in milliseconds up to an optional limit.</summary>
    public struct TimerComponent : IComponent
    {
        /// <summary>Zero means no completion limit.</summary>
        public uint LimitMilliseconds;
        public uint ElapsedMilliseconds;
        public float FractionalMilliseconds;
    }
}
