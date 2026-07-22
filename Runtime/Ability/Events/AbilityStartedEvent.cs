namespace UniGame.StaticEcs.Features
{
    using FFS.Libraries.StaticEcs;

    /// <summary>
    /// Emitted by <c>AbilityCastSystem</c> immediately after a cast-entity is spawned. The
    /// business layer (cooldown, resource cost, anti-spam) subscribes here to apply its own
    /// post-cast effects without coupling the ability slice to those concerns.
    /// </summary>
    public struct AbilityStartedEvent : IEvent
    {
        public EntityGID Caster;
        public AbilityId AbilityId;
        public EntityGID CastEntity;
    }
}
