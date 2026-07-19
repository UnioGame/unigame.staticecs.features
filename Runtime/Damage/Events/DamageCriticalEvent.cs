using FFS.Libraries.StaticEcs;

namespace UniGame.StaticEcs.Features {
    /// <summary>
    /// Raised when the critical filter amplifies an incoming damage event.
    /// </summary>
    public struct DamageCriticalEvent : IEvent {
        public EntityGID Source;
        public EntityGID Target;
        public float BaseAmount;
        public float FinalAmount;
        public float Multiplier;
        public DamageType Type;
    }
}
