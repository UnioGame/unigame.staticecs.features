namespace UniGame.StaticEcs.Features
{
    using FFS.Libraries.StaticEcs;

    /// <summary>Reports one cooldown completion; the entity may already be destroyed when read.</summary>
    public struct CooldownFinishedEvent : IEvent
    {
        public EntityGID Entity;
    }
}
