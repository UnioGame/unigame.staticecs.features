namespace UniGame.StaticEcs.Features
{
    using FFS.Libraries.StaticEcs;

    /// <summary>
    /// Marker on a sub-ability cast-entity indicating it should outlive its parent.
    /// </summary>
    public struct AbilityDetachedSubcastTag : ITag
    {
    }
}
