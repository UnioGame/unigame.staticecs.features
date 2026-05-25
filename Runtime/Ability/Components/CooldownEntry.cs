using FFS.Libraries.StaticEcs;

namespace unigame.staticecs.features {
    /// <summary>
    /// Per-caster cooldown record stored as a multi-component. <see cref="ExpiresAt"/> is compared
    /// against <c>EcsTime.Now</c>; an entry whose expiry is in the past is considered ready and is
    /// lazily reused or removed by <see cref="CooldownOperations"/>.
    /// </summary>
    public struct CooldownEntry : IMultiComponent {
        public AbilityId Id;
        public float ExpiresAt;
    }
}
