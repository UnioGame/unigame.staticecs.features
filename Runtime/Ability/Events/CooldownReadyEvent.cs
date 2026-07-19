using FFS.Libraries.StaticEcs;

namespace UniGame.StaticEcs.Features {
    /// <summary>
    /// Emitted by <see cref="CooldownOperations"/> the first time a query observes a cooldown
    /// entry whose <see cref="CooldownEntry.ExpiresAt"/> has elapsed. Useful for HUD ready-flashes;
    /// not emitted on initial registration.
    /// </summary>
    public struct CooldownReadyEvent : IEvent {
        public EntityGID Caster;
        public AbilityId AbilityId;
    }
}
