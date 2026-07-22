namespace UniGame.StaticEcs.Features
{
    using FFS.Libraries.StaticEcs;

    /// <summary>
    /// Per-caster cooldown record stored as a multi-component. <see cref="ExpiresAt"/> is compared
    /// against <c>EcsTime.Now</c>; an entry whose expiry is in the past is considered ready and is
    /// lazily reused or removed by <see cref="CooldownOperations"/>.
    /// </summary>
    public struct CooldownComponent : IMultiComponent
    {
        /// <summary>The ability whose cooldown is tracked.</summary>
        public AbilityId Id;

        /// <summary>The world time at which the ability becomes ready.</summary>
        public float ExpiresAt;
    }
}
