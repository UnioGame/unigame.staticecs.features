using FFS.Libraries.StaticEcs;

namespace UniGame.StaticEcs.Features {
    /// <summary>
    /// Marker on a cast-entity indicating it is a channel cast (not blocking the caster's
    /// foreground slot). Reserved for PR #4 — registered upfront for type stability.
    /// </summary>
    public struct AbilityChannelCastTag : ITag { }
}
