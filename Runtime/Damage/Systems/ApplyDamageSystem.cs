using FFS.Libraries.StaticEcs;

namespace UniGame.StaticEcs.Features {
    /// <summary>
    /// Consumes <see cref="IncomingDamageEvent"/>, runs the registered
    /// <c>DamageFilterChain&lt;TWorld&gt;</c>, and applies the surviving amount to the target's
    /// <see cref="HealthCharacteristic"/>. Emits the matching terminating event for each path:
    /// <see cref="DamageDodgedEvent"/>, <see cref="DamageBlockedEvent"/>, or
    /// <see cref="DamageAppliedEvent"/> with <see cref="DeathPendingTag"/> on lethal hits.
    /// </summary>
    public struct ApplyDamageSystem<TWorld> : ISystem
        where TWorld : struct, IWorldType {
        private EventReceiver<TWorld, IncomingDamageEvent> _receiver;

        public void Init() {
            _receiver = World<TWorld>.RegisterEventReceiver<IncomingDamageEvent>();
        }

        public void Update() {
            ref var chain = ref World<TWorld>.GetResource<DamageFilterChain<TWorld>>();

            foreach (var e in _receiver) {
                var ctx = DamageContext.FromEvent(in e.Value);
                chain.Apply(ref ctx);

                if (ctx.Cancelled) {
                    EmitCancellation(in ctx);
                    continue;
                }

                ApplyToTarget(ref ctx);
            }
        }

        public void Destroy() {
            World<TWorld>.DeleteEventReceiver(ref _receiver);
        }

        private static void EmitCancellation(in DamageContext ctx) {
            switch (ctx.CancelReason) {
                case DamageCancelReason.Dodged:
                    World<TWorld>.SendEvent(new DamageDodgedEvent {
                        Source = ctx.Source,
                        Target = ctx.Target,
                        Amount = ctx.OriginalAmount,
                        Type   = ctx.Type
                    });
                    break;
                case DamageCancelReason.Blocked:
                    World<TWorld>.SendEvent(new DamageBlockedEvent {
                        Source        = ctx.Source,
                        Target        = ctx.Target,
                        BlockedAmount = ctx.OriginalAmount,
                        Type          = ctx.Type
                    });
                    break;
            }
        }

        private static void ApplyToTarget(ref DamageContext ctx) {
            if (!ctx.Target.TryUnpack<TWorld>(out var target)) {
                return;
            }

            if (!target.Has<CharacteristicComponent<HealthCharacteristic>>()) {
                return;
            }

            ref var health = ref target.Ref<CharacteristicComponent<HealthCharacteristic>>();
            var killing = false;
            float appliedAmount;

            if (ctx.Type == DamageType.Healing) {
                CharacteristicOperations.AddValue<TWorld, HealthCharacteristic>(ref health, ctx.Target, ctx.Amount);
                appliedAmount = ctx.Amount;
            } else {
                if (ctx.Amount <= 0f) {
                    appliedAmount = 0f;
                } else {
                    var newValue = health.Value - ctx.Amount;
                    if (newValue < health.MinValue) {
                        newValue = health.MinValue;
                    }

                    CharacteristicOperations.SetValue<TWorld, HealthCharacteristic>(ref health, ctx.Target, newValue);
                    appliedAmount = ctx.Amount;

                    if (health.Value <= health.MinValue && !target.Has<DeathPendingTag>()) {
                        target.Set<DeathPendingTag>();
                        killing = true;
                    }
                }
            }

            World<TWorld>.SendEvent(new DamageAppliedEvent {
                Source      = ctx.Source,
                Target      = ctx.Target,
                Amount      = appliedAmount,
                Type        = ctx.Type,
                IsCritical  = ctx.IsCritical,
                KillingBlow = killing
            });
        }
    }
}
