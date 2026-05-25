using FFS.Libraries.StaticEcs;
using unigame.staticecs.Time;
using unigame.staticecs.unity;

namespace unigame.staticecs.features {
    /// <summary>
    /// Virtual-time cooldown helpers operating on a <see cref="CooldownEntry"/> multi-component.
    /// All comparisons use <c>EcsTime.Now</c>, so cooldowns automatically respect time scaling and
    /// pause without a dedicated tick system.
    /// </summary>
    public static class CooldownOperations {
        public static void Trigger<TWorld>(EntityGID caster, AbilityId id, float duration)
            where TWorld : struct, IWorldType {
            if (duration <= 0f) {
                return;
            }
            if (!caster.TryUnpack<TWorld>(out var entity)) {
                return;
            }

            var now = World<TWorld>.GetResource<EcsTime>().Now;
            var expiresAt = now + duration;

            if (!entity.Has<World<TWorld>.Multi<CooldownEntry>>()) {
                entity.Add<World<TWorld>.Multi<CooldownEntry>>();
            }

            ref var cooldowns = ref entity.Ref<World<TWorld>.Multi<CooldownEntry>>();
            for (var i = 0; i < cooldowns.Length; i++) {
                if (cooldowns[i].Id == id) {
                    var entry = cooldowns[i];
                    entry.ExpiresAt = expiresAt;
                    cooldowns[i] = entry;
                    return;
                }
            }

            cooldowns.Add(new CooldownEntry { Id = id, ExpiresAt = expiresAt });
        }

        public static bool IsReady<TWorld>(EntityGID caster, AbilityId id)
            where TWorld : struct, IWorldType {
            if (!caster.TryUnpack<TWorld>(out var entity)) {
                return false;
            }
            if (!entity.Has<World<TWorld>.Multi<CooldownEntry>>()) {
                return true;
            }

            var now = World<TWorld>.GetResource<EcsTime>().Now;
            ref var cooldowns = ref entity.Ref<World<TWorld>.Multi<CooldownEntry>>();
            for (var i = 0; i < cooldowns.Length; i++) {
                if (cooldowns[i].Id != id) {
                    continue;
                }

                if (cooldowns[i].ExpiresAt <= now) {
                    cooldowns.RemoveAtSwap(i);
                    World<TWorld>.SendEvent(new CooldownReadyEvent { Caster = caster, AbilityId = id });
                    return true;
                }
                return false;
            }
            return true;
        }

        public static float Remaining<TWorld>(EntityGID caster, AbilityId id)
            where TWorld : struct, IWorldType {
            if (!caster.TryUnpack<TWorld>(out var entity)) {
                return 0f;
            }
            if (!entity.Has<World<TWorld>.Multi<CooldownEntry>>()) {
                return 0f;
            }

            var now = World<TWorld>.GetResource<EcsTime>().Now;
            ref var cooldowns = ref entity.Ref<World<TWorld>.Multi<CooldownEntry>>();
            for (var i = 0; i < cooldowns.Length; i++) {
                if (cooldowns[i].Id == id) {
                    var remaining = cooldowns[i].ExpiresAt - now;
                    return remaining > 0f ? remaining : 0f;
                }
            }
            return 0f;
        }

        public static bool Reset<TWorld>(EntityGID caster, AbilityId id)
            where TWorld : struct, IWorldType {
            if (!caster.TryUnpack<TWorld>(out var entity)) {
                return false;
            }
            if (!entity.Has<World<TWorld>.Multi<CooldownEntry>>()) {
                return false;
            }

            ref var cooldowns = ref entity.Ref<World<TWorld>.Multi<CooldownEntry>>();
            for (var i = 0; i < cooldowns.Length; i++) {
                if (cooldowns[i].Id == id) {
                    cooldowns.RemoveAtSwap(i);
                    return true;
                }
            }
            return false;
        }

        // --- Main-default overloads ---

        public static void Trigger(EntityGID caster, AbilityId id, float duration) => Trigger<Main>(caster, id, duration);
        public static bool IsReady(EntityGID caster, AbilityId id) => IsReady<Main>(caster, id);
        public static float Remaining(EntityGID caster, AbilityId id) => Remaining<Main>(caster, id);
        public static bool Reset(EntityGID caster, AbilityId id) => Reset<Main>(caster, id);
    }
}
