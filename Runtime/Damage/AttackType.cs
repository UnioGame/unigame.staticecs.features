namespace UniGame.StaticEcs.Features
{
    /// <summary>
    /// How a damage event is delivered. Decides whether the hit checks of the filter chain (dodge, block and
    /// critical) run for it, see <see cref="AttackTypeRules.RollsHitChecks"/>. Armor and shield do not depend on it.
    /// </summary>
    public enum AttackType : byte
    {
        /// <summary>A direct strike at one target; every hit check runs.</summary>
        Hit = 0,

        /// <summary>A strike by a flying body; every hit check runs.</summary>
        Projectile = 1,

        /// <summary>
        /// Damage of an area or of a running status on each activation, for example a poison tick. It cannot be
        /// dodged, blocked or critical, so a periodic effect does not miss every second.
        /// </summary>
        Area = 2,

        /// <summary>Damage of an area whose every hit rolls like a <see cref="Hit"/>: dodge, block and critical.</summary>
        AreaHit = 3,
    }

    /// <summary>The hit-check rules of <see cref="AttackType"/>.</summary>
    public static class AttackTypeRules
    {
        /// <summary>Returns whether dodge, block and critical rolls run for the attack type.</summary>
        public static bool RollsHitChecks(this AttackType type)
        {
            return type != AttackType.Area;
        }
    }
}
