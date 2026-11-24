namespace UniGame.StaticEcs.Features
{
    using FFS.Libraries.StaticEcs;

    /// <summary>
    /// Set by the apply step when an entity reaches zero health. The game's death feature
    /// consumes this tag and decides what the lethal hit means for the entity.
    /// </summary>
    public struct DeathPendingTag : ITag
    {
    }
}
