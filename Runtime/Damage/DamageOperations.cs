namespace UniGame.StaticEcs.Features
{
    using FFS.Libraries.StaticEcs;
    using Unity;

    /// <summary>
    /// Public entry points for raising damage and healing. Both helpers send
    /// <see cref="IncomingDamageEvent"/>; the difference is only the <see cref="DamageType"/>
    /// pre-set on the event so callers cannot accidentally route healing through the regular
    /// damage flow.
    /// </summary>
    public static class DamageOperations
    {
        /// <summary>
        /// Runs the damage filter chain over the event and applies the surviving amount to the target at once,
        /// raising the same terminating event as <c>ApplyDamageSystem</c>. Use it when the caller needs the
        /// outcome (for example a critical hit) in the same tick.
        /// </summary>
        public static DamageResolution Resolve<TWorld>(in IncomingDamageEvent damage)
            where TWorld : struct, IWorldType
        {
            var chain = World<TWorld>.GetResource<DamageFilterChain<TWorld>>();
            var ctx = DamageContext.FromEvent(in damage);
            chain.Apply(ref ctx);

            if (ctx.Cancelled)
            {
                EmitCancellation<TWorld>(in ctx);
                return new DamageResolution { CancelReason = ctx.CancelReason };
            }

            return ApplyToTarget<TWorld>(ref ctx);
        }

        /// <summary>
        /// Rolls the critical chance of the source once. Pair it with <see cref="IncomingDamageEvent.CriticalRolled"/>
        /// to give every target of one hit the same outcome.
        /// </summary>
        public static bool RollCritical<TWorld>(EntityGID source)
            where TWorld : struct, IWorldType
        {
            if (!DamageCharacteristicHelper.TryReadValue<TWorld, CriticalChanceCharacteristic>(source, out var chance) ||
                chance <= 0f)
                return false;

            return World<TWorld>.GetResource<IDamageRng>().RollChance(chance);
        }

        public static bool RaiseDamage<TWorld>(
            EntityGID source,
            EntityGID target,
            float amount,
            DamageType type = DamageType.Physical,
            bool forceCritical = false
        )
            where TWorld : struct, IWorldType
        {
            if (amount <= 0f)
                return false;

            return World<TWorld>.SendEvent(
                new IncomingDamageEvent
                {
                    Source = source,
                    Target = target,
                    Amount = amount,
                    Type = type,
                    ForceCritical = forceCritical,
                }
            );
        }

        public static bool RaiseHealing<TWorld>(EntityGID source, EntityGID target, float amount)
            where TWorld : struct, IWorldType
        {
            if (amount <= 0f)
                return false;

            return World<TWorld>.SendEvent(
                new IncomingDamageEvent
                {
                    Source = source,
                    Target = target,
                    Amount = amount,
                    Type = DamageType.Healing,
                    ForceCritical = false,
                }
            );
        }

        private static void EmitCancellation<TWorld>(in DamageContext ctx)
            where TWorld : struct, IWorldType
        {
            switch (ctx.CancelReason)
            {
                case DamageCancelReason.Dodged:
                    World<TWorld>.SendEvent(
                        new DamageDodgedEvent
                        {
                            Source = ctx.Source,
                            Target = ctx.Target,
                            Amount = ctx.OriginalAmount,
                            Type = ctx.Type,
                        }
                    );
                    break;
                case DamageCancelReason.Blocked:
                    World<TWorld>.SendEvent(
                        new DamageBlockedEvent
                        {
                            Source = ctx.Source,
                            Target = ctx.Target,
                            BlockedAmount = ctx.OriginalAmount,
                            Type = ctx.Type,
                        }
                    );
                    break;
            }
        }

        private static DamageResolution ApplyToTarget<TWorld>(ref DamageContext ctx)
            where TWorld : struct, IWorldType
        {
            var resolution = new DamageResolution { IsCritical = ctx.IsCritical, Amount = ctx.Amount };
            if (!ctx.Target.TryUnpack<TWorld>(out var target))
                return resolution;

            if (!target.Has<CharacteristicComponent<HealthCharacteristic>>())
                return resolution;

            ref var health = ref target.Ref<CharacteristicComponent<HealthCharacteristic>>();
            var killing = false;
            var beforeValue = health.Value;

            if (ctx.Type == DamageType.Healing)
                CharacteristicOperations.AddValue<TWorld, HealthCharacteristic>(
                    ref health,
                    ctx.Target,
                    ctx.Amount
                );
            else if (ctx.Amount > 0f)
            {
                var newValue = health.Value - ctx.Amount;
                if (newValue < health.MinValue)
                    newValue = health.MinValue;

                CharacteristicOperations.SetValue<TWorld, HealthCharacteristic>(
                    ref health,
                    ctx.Target,
                    newValue
                );

                if (health.Value <= health.MinValue && !target.Has<DeathPendingTag>())
                {
                    target.Set<DeathPendingTag>();
                    killing = true;
                }
            }

            var appliedAmount = ctx.Type == DamageType.Healing
                ? health.Value - beforeValue
                : beforeValue - health.Value;

            World<TWorld>.SendEvent(
                new DamageAppliedEvent
                {
                    Source = ctx.Source,
                    Target = ctx.Target,
                    Amount = ctx.Amount,
                    AppliedAmount = appliedAmount,
                    Type = ctx.Type,
                    IsCritical = ctx.IsCritical,
                    KillingBlow = killing,
                }
            );

            resolution.IsApplied = true;
            resolution.AppliedAmount = appliedAmount;
            resolution.KillingBlow = killing;
            return resolution;
        }

        // --- Main-default overloads ---

        public static bool RaiseDamage(
            EntityGID source,
            EntityGID target,
            float amount,
            DamageType type = DamageType.Physical,
            bool forceCritical = false
        ) => RaiseDamage<Main>(source, target, amount, type, forceCritical);

        public static bool RaiseHealing(EntityGID source, EntityGID target, float amount) =>
            RaiseHealing<Main>(source, target, amount);
    }
}
