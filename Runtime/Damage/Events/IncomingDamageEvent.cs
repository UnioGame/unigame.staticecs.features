namespace UniGame.StaticEcs.Features
{
    using FFS.Libraries.StaticEcs;

    /// <summary>
    /// Single entry point for the damage pipeline. Emitted by <c>DamageOperations.RaiseDamage</c>
    /// and consumed by <c>ApplyDamageSystem</c> which runs the registered filter chain.
    /// </summary>
    public struct IncomingDamageEvent : IEvent
    {
        public EntityGID Source;
        public EntityGID Target;
        public float Amount;
        public DamageType Type;
        public bool ForceCritical;

        /// <summary>
        /// The caller already decided whether the hit is critical (<see cref="ForceCritical"/>), so the critical
        /// filter does not roll. Lets one roll cover every target of an area hit.
        /// </summary>
        public bool CriticalRolled;

        /// <summary>How the damage is delivered; <see cref="AttackType.Area"/> skips dodge, block and critical.</summary>
        public AttackType AttackType;
    }
}
