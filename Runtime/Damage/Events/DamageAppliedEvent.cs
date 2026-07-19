using FFS.Libraries.StaticEcs;

namespace UniGame.StaticEcs.Features {
    /// <summary>
    /// Raised after the apply step mutates the target's health. <see cref="KillingBlow"/> is
    /// true when the application drops the target to or below zero health, in which case
    /// <see cref="DeathPendingTag"/> is also set on the target.
    /// </summary>
    public struct DamageAppliedEvent : IEvent {
        public EntityGID Source;
        public EntityGID Target;
        public float Amount;
        public DamageType Type;
        public bool IsCritical;
        public bool KillingBlow;
    }
}
