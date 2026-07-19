using FFS.Libraries.StaticEcs;

namespace UniGame.StaticEcs.Features {
    /// <summary>
    /// Raised when the dodge filter cancels an incoming damage event.
    /// </summary>
    public struct DamageDodgedEvent : IEvent {
        public EntityGID Source;
        public EntityGID Target;
        public float Amount;
        public DamageType Type;
    }
}
