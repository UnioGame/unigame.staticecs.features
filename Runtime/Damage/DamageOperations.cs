using FFS.Libraries.StaticEcs;
using unigame.staticecs.unity;

namespace unigame.staticecs.features {
    /// <summary>
    /// Public entry points for raising damage and healing. Both helpers send
    /// <see cref="IncomingDamageEvent"/>; the difference is only the <see cref="DamageType"/>
    /// pre-set on the event so callers cannot accidentally route healing through the regular
    /// damage flow.
    /// </summary>
    public static class DamageOperations {
        public static bool RaiseDamage<TWorld>(
            EntityGID source,
            EntityGID target,
            float amount,
            DamageType type = DamageType.Physical,
            bool forceCritical = false)
            where TWorld : struct, IWorldType {
            if (amount <= 0f) {
                return false;
            }

            return World<TWorld>.SendEvent(new IncomingDamageEvent {
                Source        = source,
                Target        = target,
                Amount        = amount,
                Type          = type,
                ForceCritical = forceCritical
            });
        }

        public static bool RaiseHealing<TWorld>(
            EntityGID source,
            EntityGID target,
            float amount)
            where TWorld : struct, IWorldType {
            if (amount <= 0f) {
                return false;
            }

            return World<TWorld>.SendEvent(new IncomingDamageEvent {
                Source        = source,
                Target        = target,
                Amount        = amount,
                Type          = DamageType.Healing,
                ForceCritical = false
            });
        }

        // --- Main-default overloads ---

        public static bool RaiseDamage(
            EntityGID source,
            EntityGID target,
            float amount,
            DamageType type = DamageType.Physical,
            bool forceCritical = false)
            => RaiseDamage<Main>(source, target, amount, type, forceCritical);

        public static bool RaiseHealing(EntityGID source, EntityGID target, float amount)
            => RaiseHealing<Main>(source, target, amount);
    }
}
