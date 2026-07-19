using FFS.Libraries.StaticEcs;

namespace UniGame.StaticEcs.Features {
    /// <summary>
    /// Single entry point for the damage pipeline. Emitted by <c>DamageOperations.RaiseDamage</c>
    /// and consumed by <c>ApplyDamageSystem</c> which runs the registered filter chain.
    /// </summary>
    public struct IncomingDamageEvent : IEvent {
        public EntityGID Source;
        public EntityGID Target;
        public float Amount;
        public DamageType Type;
        public bool ForceCritical;
    }
}
