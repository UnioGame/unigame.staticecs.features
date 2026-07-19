using FFS.Libraries.StaticEcs;

namespace UniGame.StaticEcs.Features {
    /// <summary>
    /// Reason a damage event was cancelled by a filter. Read by <c>ApplyDamageSystem</c> to
    /// decide which terminating event to raise instead of <see cref="DamageAppliedEvent"/>.
    /// </summary>
    public enum DamageCancelReason : byte {
        None    = 0,
        Dodged  = 1,
        Blocked = 2
    }

    /// <summary>
    /// Working state passed by reference through the damage filter chain. Lives on the stack of
    /// <c>ApplyDamageSystem.Update</c>; filters read and mutate the same instance, then the
    /// apply step consumes the final values.
    /// </summary>
    public ref struct DamageContext {
        public EntityGID Source;
        public EntityGID Target;
        public float Amount;
        public DamageType Type;
        public bool ForceCritical;
        public bool IsCritical;
        public bool Cancelled;
        public DamageCancelReason CancelReason;
        public float ShieldAbsorbed;
        public float OriginalAmount;

        public static DamageContext FromEvent(in IncomingDamageEvent evt) {
            return new DamageContext {
                Source         = evt.Source,
                Target         = evt.Target,
                Amount         = evt.Amount,
                Type           = evt.Type,
                ForceCritical  = evt.ForceCritical,
                IsCritical     = false,
                Cancelled      = false,
                CancelReason   = DamageCancelReason.None,
                ShieldAbsorbed = 0f,
                OriginalAmount = evt.Amount
            };
        }
    }
}
