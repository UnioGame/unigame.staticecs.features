namespace UniGame.StaticEcs.Features
{
    using FFS.Libraries.StaticEcs;

    /// <summary>
    /// Cancels the damage event when a roll against the target's
    /// <see cref="DodgeChanceCharacteristic"/> succeeds. Healing events bypass the filter.
    /// </summary>
    public sealed class DodgeFilter<TWorld> : IDamageFilter<TWorld>
        where TWorld : struct, IWorldType
    {
        public void Apply(ref DamageContext ctx)
        {
            if (ctx.Cancelled || ctx.Type == DamageType.Healing)
            {
                return;
            }

            if (
                !DamageCharacteristicHelper.TryReadValue<TWorld, DodgeChanceCharacteristic>(
                    ctx.Target,
                    out var chance
                )
            )
            {
                return;
            }

            if (chance <= 0f)
            {
                return;
            }

            ref var rng = ref World<TWorld>.GetResource<IDamageRng>();
            if (!rng.RollChance(chance))
            {
                return;
            }

            ctx.Cancelled = true;
            ctx.CancelReason = DamageCancelReason.Dodged;
        }
    }
}
