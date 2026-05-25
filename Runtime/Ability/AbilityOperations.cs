using FFS.Libraries.StaticEcs;
using unigame.staticecs.unity;

namespace unigame.staticecs.features {
    /// <summary>
    /// Public entry points for the ability slice: roster equip/unequip, readiness queries,
    /// cast requests and cancellation. All cast requests funnel through
    /// <see cref="CastAbilityEvent"/> so authoring code stays declarative; the actual phase
    /// machine lives in <c>AbilityCastSystem</c> + <c>AbilityTickSystem</c>.
    /// </summary>
    public static class AbilityOperations {
        public static bool Equip<TWorld>(EntityGID caster, AbilityId id)
            where TWorld : struct, IWorldType {
            if (!caster.TryUnpack<TWorld>(out var entity)) {
                return false;
            }

            if (!entity.Has<World<TWorld>.Multi<AbilityRosterEntry>>()) {
                entity.Add<World<TWorld>.Multi<AbilityRosterEntry>>();
            }

            ref var roster = ref entity.Ref<World<TWorld>.Multi<AbilityRosterEntry>>();
            for (var i = 0; i < roster.Length; i++) {
                if (roster[i].Id == id) {
                    return false;
                }
            }

            roster.Add(new AbilityRosterEntry { Id = id });
            return true;
        }

        public static bool Unequip<TWorld>(EntityGID caster, AbilityId id)
            where TWorld : struct, IWorldType {
            if (!caster.TryUnpack<TWorld>(out var entity)) {
                return false;
            }
            if (!entity.Has<World<TWorld>.Multi<AbilityRosterEntry>>()) {
                return false;
            }

            ref var roster = ref entity.Ref<World<TWorld>.Multi<AbilityRosterEntry>>();
            for (var i = 0; i < roster.Length; i++) {
                if (roster[i].Id == id) {
                    roster.RemoveAtSwap(i);
                    return true;
                }
            }
            return false;
        }

        public static bool HasAbility<TWorld>(EntityGID caster, AbilityId id)
            where TWorld : struct, IWorldType {
            if (!caster.TryUnpack<TWorld>(out var entity)) {
                return false;
            }
            if (!entity.Has<World<TWorld>.Multi<AbilityRosterEntry>>()) {
                return false;
            }

            ref readonly var roster = ref entity.Read<World<TWorld>.Multi<AbilityRosterEntry>>();
            for (var i = 0; i < roster.Length; i++) {
                if (roster[i].Id == id) {
                    return true;
                }
            }
            return false;
        }

        public static bool IsCasting<TWorld>(EntityGID caster)
            where TWorld : struct, IWorldType {
            if (!caster.TryUnpack<TWorld>(out var entity)) {
                return false;
            }
            return entity.Has<AbilityCastComponent>();
        }

        public static bool IsReady<TWorld>(EntityGID caster, AbilityId id)
            where TWorld : struct, IWorldType {
            if (IsCasting<TWorld>(caster)) {
                return false;
            }
            if (!HasAbility<TWorld>(caster, id)) {
                return false;
            }
            if (!World<TWorld>.HasResource<AbilityRegistry<TWorld>>()) {
                return false;
            }
            if (!World<TWorld>.GetResource<AbilityRegistry<TWorld>>().Contains(id)) {
                return false;
            }
            return CooldownOperations.IsReady<TWorld>(caster, id);
        }

        public static bool TryStartCast<TWorld>(EntityGID caster, AbilityId id, EntityGID target = default)
            where TWorld : struct, IWorldType {
            if (!IsReady<TWorld>(caster, id)) {
                return false;
            }

            return World<TWorld>.SendEvent(new CastAbilityEvent {
                Caster = caster,
                AbilityId = id,
                Target = target,
            });
        }

        public static bool Cancel<TWorld>(EntityGID caster)
            where TWorld : struct, IWorldType {
            if (!caster.TryUnpack<TWorld>(out var entity)) {
                return false;
            }
            if (!entity.Has<AbilityCastComponent>()) {
                return false;
            }

            ref readonly var cast = ref entity.Read<AbilityCastComponent>();
            var abilityId = cast.AbilityId;
            var phase = cast.Phase;

            entity.Delete<AbilityCastComponent>();

            World<TWorld>.SendEvent(new AbilityStateChangedEvent {
                Caster = caster,
                AbilityId = abilityId,
                Phase = phase,
                Reason = AbilityChangeReason.Cancelled,
            });
            return true;
        }

        // --- Main-default overloads ---

        public static bool Equip(EntityGID caster, AbilityId id) => Equip<Main>(caster, id);
        public static bool Unequip(EntityGID caster, AbilityId id) => Unequip<Main>(caster, id);
        public static bool HasAbility(EntityGID caster, AbilityId id) => HasAbility<Main>(caster, id);
        public static bool IsCasting(EntityGID caster) => IsCasting<Main>(caster);
        public static bool IsReady(EntityGID caster, AbilityId id) => IsReady<Main>(caster, id);
        public static bool TryStartCast(EntityGID caster, AbilityId id, EntityGID target = default) => TryStartCast<Main>(caster, id, target);
        public static bool Cancel(EntityGID caster) => Cancel<Main>(caster);
    }
}
