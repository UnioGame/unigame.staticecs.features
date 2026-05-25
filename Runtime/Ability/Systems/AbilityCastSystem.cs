using FFS.Libraries.StaticEcs;

namespace unigame.staticecs.features {
    /// <summary>
    /// Validates incoming <see cref="CastAbilityEvent"/>s and starts a cast by attaching
    /// <see cref="AbilityCastComponent"/>. Re-checks roster, cooldown and concurrent-cast guards
    /// because events queued in the same frame may invalidate one another.
    /// </summary>
    public sealed class AbilityCastSystem<TWorld> : ISystem
        where TWorld : struct, IWorldType {
        private EventReceiver<TWorld, CastAbilityEvent> _receiver;

        public void Init() {
            _receiver = World<TWorld>.RegisterEventReceiver<CastAbilityEvent>();
        }

        public void Update() {
            if (!World<TWorld>.HasResource<AbilityRegistry<TWorld>>()) {
                foreach (var _ in _receiver) { }
                return;
            }

            var registry = World<TWorld>.GetResource<AbilityRegistry<TWorld>>();

            foreach (var e in _receiver) {
                var ev = e.Value;
                if (!ev.Caster.TryUnpack<TWorld>(out var caster)) {
                    continue;
                }
                if (caster.Has<AbilityCastComponent>()) {
                    continue;
                }
                if (!AbilityOperations.HasAbility<TWorld>(ev.Caster, ev.AbilityId)) {
                    continue;
                }
                if (!registry.TryGet(ev.AbilityId, out var def)) {
                    continue;
                }
                if (!CooldownOperations.IsReady<TWorld>(ev.Caster, ev.AbilityId)) {
                    continue;
                }

                ResolveStartingPhase(def, out var phase, out var duration);

                if (duration <= 0f && phase == AbilityPhase.Recovering) {
                    InvokeHandler(registry, ev.AbilityId, ev.Caster, ev.Target);
                    if (def.Cooldown > 0f) {
                        CooldownOperations.Trigger<TWorld>(ev.Caster, ev.AbilityId, def.Cooldown);
                    }
                    World<TWorld>.SendEvent(new AbilityStateChangedEvent {
                        Caster = ev.Caster,
                        AbilityId = ev.AbilityId,
                        Phase = phase,
                        Reason = AbilityChangeReason.Started,
                    });
                    World<TWorld>.SendEvent(new AbilityStateChangedEvent {
                        Caster = ev.Caster,
                        AbilityId = ev.AbilityId,
                        Phase = phase,
                        Reason = AbilityChangeReason.Completed,
                    });
                    continue;
                }

                var component = new AbilityCastComponent {
                    AbilityId = ev.AbilityId,
                    Phase = phase,
                    TimeLeft = duration,
                    Target = ev.Target,
                };
                caster.Set(component);

                World<TWorld>.SendEvent(new AbilityStateChangedEvent {
                    Caster = ev.Caster,
                    AbilityId = ev.AbilityId,
                    Phase = phase,
                    Reason = AbilityChangeReason.Started,
                });

                if (phase != AbilityPhase.Charging) {
                    InvokeHandler(registry, ev.AbilityId, ev.Caster, ev.Target);
                }
            }
        }

        private static void InvokeHandler(
            AbilityRegistry<TWorld> registry,
            AbilityId abilityId,
            EntityGID caster,
            EntityGID target) {
            if (target.TryUnpack<TWorld>(out _)) {
                System.Span<EntityGID> buffer = stackalloc EntityGID[1];
                buffer[0] = target;
                registry.Invoke(abilityId, caster, buffer);
            } else {
                registry.Invoke(abilityId, caster, System.ReadOnlySpan<EntityGID>.Empty);
            }
        }

        public void Destroy() {
            World<TWorld>.DeleteEventReceiver(ref _receiver);
        }

        private static void ResolveStartingPhase(AbilityDefinition def, out AbilityPhase phase, out float duration) {
            if (def.ChargeDuration > 0f) {
                phase = AbilityPhase.Charging;
                duration = def.ChargeDuration;
                return;
            }
            if (def.CastDuration > 0f) {
                phase = AbilityPhase.Casting;
                duration = def.CastDuration;
                return;
            }
            phase = AbilityPhase.Recovering;
            duration = def.RecoverDuration;
        }
    }
}
