using FFS.Libraries.StaticEcs;
 

namespace UniGame.StaticEcs.Features {
    using Unity;

    /// <summary>
    /// Convenience entry point for the heal-over-time effect: stamps
    /// <see cref="HealOverTimeData"/> on the target, then forwards to the generic
    /// <c>EffectOperations.Apply</c>.
    /// </summary>
    public static class HealOverTimeOperations {
        public static bool Apply<TWorld>(
            EntityGID target,
            EntityGID source,
            float healPerTick,
            float duration,
            float period,
            float delay = 0f)
            where TWorld : struct, IWorldType {
            if (healPerTick <= 0f || duration <= 0f || period <= 0f) {
                return false;
            }

            if (!target.TryUnpack<TWorld>(out var entity)) {
                return false;
            }

            if (!entity.Has<HealOverTimeData>()) {
                entity.Add<HealOverTimeData>();
            }

            ref var data = ref entity.Ref<HealOverTimeData>();
            data.HealPerTick = healPerTick;

            return EffectOperations.Apply<TWorld, HealOverTimeEffect>(target, source, duration, period, delay);
        }

        // --- Main-default overload ---

        public static bool Apply(
            EntityGID target,
            EntityGID source,
            float healPerTick,
            float duration,
            float period,
            float delay = 0f)
            => Apply<Main>(target, source, healPerTick, duration, period, delay);
    }
}
