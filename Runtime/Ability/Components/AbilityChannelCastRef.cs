using FFS.Libraries.StaticEcs;

namespace UniGame.StaticEcs.Features {
    /// <summary>
    /// Per-caster list of currently running channel cast-entities. Reserved for the channel
    /// cast model that lands fully in PR #4 — registered upfront so the type set stays stable
    /// across PRs.
    /// </summary>
    public struct AbilityChannelCastRef : IMultiComponent {
        public EntityGID Cast;
    }
}
