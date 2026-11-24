namespace UniGame.StaticEcs.Features
{
    using FFS.Libraries.StaticEcs;

    /// <summary>Tracks the remaining milliseconds of an entity cooldown.</summary>
    public struct CooldownRemainingComponent : IComponent
    {
        public uint RemainingMilliseconds;
        public float FractionalMilliseconds;
    }
}
