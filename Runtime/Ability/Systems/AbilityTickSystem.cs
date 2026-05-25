using FFS.Libraries.StaticEcs;
using unigame.staticecs.Time;

namespace unigame.staticecs.features {
    /// <summary>
    /// Advances every active <see cref="AbilityCastComponent"/> through Charging → Casting →
    /// Recovering using <c>EcsTime.DeltaTime</c>. Invokes the registered handler exactly once,
    /// at the transition out of <see cref="AbilityPhase.Charging"/> (or immediately at Start when
    /// no Charging phase exists), and triggers cooldown + completion at the end of Recovering.
    /// </summary>
    public sealed class AbilityTickSystem<TWorld> : ISystem
        where TWorld : struct, IWorldType {
        public void Update() {
            var dt = World<TWorld>.GetResource<EcsTime>().DeltaTime;
            if (dt <= 0f) {
                return;
            }
            if (!World<TWorld>.HasResource<AbilityRegistry<TWorld>>()) {
                return;
            }

            var registry = World<TWorld>.GetResource<AbilityRegistry<TWorld>>();

            foreach (var entity in World<TWorld>
                         .Query<All<AbilityCastComponent>>()
                         .Entities()) {
                ref var cast = ref entity.Mut<AbilityCastComponent>();
                cast.TimeLeft -= dt;
                if (cast.TimeLeft > 0f) {
                    continue;
                }

                if (!registry.TryGet(cast.AbilityId, out var def)) {
                    entity.Delete<AbilityCastComponent>();
                    continue;
                }

                AdvancePhase(entity.GID, ref cast, def, registry);
            }
        }

        private static void AdvancePhase(
            EntityGID casterGid,
            ref AbilityCastComponent cast,
            AbilityDefinition def,
            AbilityRegistry<TWorld> registry) {

            var prevPhase = cast.Phase;
            var nextPhase = ResolveNextPhase(prevPhase, def, out var nextDuration, out var done);

            if (prevPhase == AbilityPhase.Charging) {
                InvokeHandler(registry, cast.AbilityId, casterGid, cast.Target);
            }

            if (done) {
                CompleteCast(casterGid, cast.AbilityId, def);
                return;
            }

            cast.Phase = nextPhase;
            cast.TimeLeft = nextDuration;

            World<TWorld>.SendEvent(new AbilityStateChangedEvent {
                Caster = casterGid,
                AbilityId = cast.AbilityId,
                Phase = nextPhase,
                Reason = AbilityChangeReason.PhaseAdvanced,
            });
        }

        private static AbilityPhase ResolveNextPhase(
            AbilityPhase current,
            AbilityDefinition def,
            out float duration,
            out bool done) {

            done = false;
            switch (current) {
                case AbilityPhase.Charging:
                    if (def.CastDuration > 0f) {
                        duration = def.CastDuration;
                        return AbilityPhase.Casting;
                    }
                    if (def.RecoverDuration > 0f) {
                        duration = def.RecoverDuration;
                        return AbilityPhase.Recovering;
                    }
                    duration = 0f;
                    done = true;
                    return AbilityPhase.Recovering;
                case AbilityPhase.Casting:
                    if (def.RecoverDuration > 0f) {
                        duration = def.RecoverDuration;
                        return AbilityPhase.Recovering;
                    }
                    duration = 0f;
                    done = true;
                    return AbilityPhase.Recovering;
                default:
                    duration = 0f;
                    done = true;
                    return AbilityPhase.Recovering;
            }
        }

        private static void CompleteCast(EntityGID casterGid, AbilityId abilityId, AbilityDefinition def) {
            if (casterGid.TryUnpack<TWorld>(out var caster) && caster.Has<AbilityCastComponent>()) {
                caster.Delete<AbilityCastComponent>();
            }

            if (def.Cooldown > 0f) {
                CooldownOperations.Trigger<TWorld>(casterGid, abilityId, def.Cooldown);
            }

            World<TWorld>.SendEvent(new AbilityStateChangedEvent {
                Caster = casterGid,
                AbilityId = abilityId,
                Phase = AbilityPhase.Recovering,
                Reason = AbilityChangeReason.Completed,
            });
        }

        private static void InvokeHandler(
            AbilityRegistry<TWorld> registry,
            AbilityId abilityId,
            EntityGID casterGid,
            EntityGID target) {

            if (target.TryUnpack<TWorld>(out _)) {
                System.Span<EntityGID> buffer = stackalloc EntityGID[1];
                buffer[0] = target;
                registry.Invoke(abilityId, casterGid, buffer);
            } else {
                registry.Invoke(abilityId, casterGid, System.ReadOnlySpan<EntityGID>.Empty);
            }
        }
    }
}
