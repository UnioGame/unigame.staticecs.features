namespace UniGame.StaticEcs.Features
{
    using FFS.Libraries.StaticEcs;

    /// <summary>Requests a restart of the target entity's configured cooldown.</summary>
    public struct RestartCooldownRequest : IEvent
    {
        public EntityGID Target;
    }
}
