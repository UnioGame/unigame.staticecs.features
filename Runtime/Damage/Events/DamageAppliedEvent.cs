namespace UniGame.StaticEcs.Features
{
    using FFS.Libraries.StaticEcs;

    /// <summary>
    /// Raised after the apply step mutates the target's health. <see cref="KillingBlow"/> is
    /// true when the application drops the target to or below zero health, in which case
    /// <see cref="DeathPendingTag"/> is also set on the target.
    /// </summary>
    public struct DamageAppliedEvent : IEvent
    {
        public EntityGID Source;
        public EntityGID Target;

        /// <summary>Filtered amount requested from the health transition.</summary>
        public float Amount;

        /// <summary>Actual positive health delta after min/max clamping.</summary>
        public float AppliedAmount;

        public DamageType Type;
        public bool IsCritical;
        public bool KillingBlow;
    }
}
