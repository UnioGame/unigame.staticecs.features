namespace UniGame.StaticEcs.Features
{
    using FFS.Libraries.StaticEcs;

    /// <summary>
    /// Multiplicatively reduces physical-typed damage by the target's
    /// <see cref="ArmorResistCharacteristic"/> value (clamped to [0, 1]). Other damage types
    /// pass through unchanged.
    /// </summary>
    public sealed class ArmorResistFilter<TWorld> : IDamageFilter<TWorld>
        where TWorld : struct, IWorldType
    {
        public void Apply(ref DamageContext ctx)
        {
            if (ctx.Cancelled || ctx.Type != DamageType.Physical || ctx.Amount <= 0f)
            {
                return;
            }

            if (
                !DamageCharacteristicHelper.TryReadValue<TWorld, ArmorResistCharacteristic>(
                    ctx.Target,
                    out var resist
                )
            )
            {
                return;
            }

            if (resist <= 0f)
            {
                return;
            }

            if (resist > 1f)
            {
                resist = 1f;
            }

            ctx.Amount *= 1f - resist;
        }
    }
}
