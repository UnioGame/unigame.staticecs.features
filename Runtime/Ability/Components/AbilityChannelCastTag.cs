namespace UniGame.StaticEcs.Features
{
    using FFS.Libraries.StaticEcs;

    /// <summary>
    /// Marker on a cast-entity indicating it is a channel cast that does not block the caster's
    /// foreground slot.
    /// </summary>
    public struct AbilityChannelCastTag : ITag
    {
    }
}
