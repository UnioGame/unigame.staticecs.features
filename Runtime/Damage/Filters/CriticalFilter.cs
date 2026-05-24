using FFS.Libraries.StaticEcs;

namespace unigame.staticecs.features {
    /// <summary>
    /// Amplifies the damage amount when <see cref="DamageContext.ForceCritical"/> is set or the
    /// crit roll succeeds against the source's <see cref="CriticalChanceCharacteristic"/>. The
    /// multiplier is read from <see cref="CriticalMultiplierCharacteristic"/> on the source and
    /// falls back to <see cref="DefaultMultiplier"/> when the source has no value configured.
    /// </summary>
    public sealed class CriticalFilter<TWorld> : IDamageFilter<TWorld>
        where TWorld : struct, IWorldType {
        public const float DefaultMultiplier = 2f;

        public void Apply(ref DamageContext ctx) {
            if (ctx.Cancelled || ctx.Type == DamageType.Healing || ctx.Amount <= 0f) {
                return;
            }

            var roll = ctx.ForceCritical;
            if (!roll) {
                if (!DamageCharacteristicHelper.TryReadValue<TWorld, CriticalChanceCharacteristic>(ctx.Source, out var chance)) {
                    return;
                }

                if (chance <= 0f) {
                    return;
                }

                ref var rng = ref World<TWorld>.GetResource<IDamageRng>();
                roll = rng.RollChance(chance);
            }

            if (!roll) {
                return;
            }

            var multiplier = DefaultMultiplier;
            if (DamageCharacteristicHelper.TryReadValue<TWorld, CriticalMultiplierCharacteristic>(ctx.Source, out var configured) && configured > 0f) {
                multiplier = configured;
            }

            var baseAmount = ctx.Amount;
            ctx.Amount *= multiplier;
            ctx.IsCritical = true;

            World<TWorld>.SendEvent(new DamageCriticalEvent {
                Source      = ctx.Source,
                Target      = ctx.Target,
                BaseAmount  = baseAmount,
                FinalAmount = ctx.Amount,
                Multiplier  = multiplier,
                Type        = ctx.Type
            });
        }
    }
}
