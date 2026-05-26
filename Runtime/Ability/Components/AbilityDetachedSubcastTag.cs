using FFS.Libraries.StaticEcs;

namespace unigame.staticecs.features {
    /// <summary>
    /// Marker on a sub-ability cast-entity indicating it should outlive its parent. Reserved
    /// for PR #4 — registered upfront for type stability.
    /// </summary>
    public struct AbilityDetachedSubcastTag : ITag { }
}
