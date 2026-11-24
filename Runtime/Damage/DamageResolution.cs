namespace UniGame.StaticEcs.Features
{
    /// <summary>Outcome of one damage event resolved by <see cref="DamageOperations.Resolve{TWorld}"/>.</summary>
    public struct DamageResolution
    {
        /// <summary>Why a filter cancelled the event; <see cref="DamageCancelReason.None"/> when it was not cancelled.</summary>
        public DamageCancelReason CancelReason;

        /// <summary>True when the amount reached the health of the target.</summary>
        public bool IsApplied;

        public bool IsCritical;

        /// <summary>Filtered amount requested from the health transition.</summary>
        public float Amount;

        /// <summary>Actual positive health delta after min/max clamping.</summary>
        public float AppliedAmount;

        public bool KillingBlow;
    }
}
