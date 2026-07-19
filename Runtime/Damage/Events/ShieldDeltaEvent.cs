using FFS.Libraries.StaticEcs;

namespace UniGame.StaticEcs.Features {
    /// <summary>
    /// Raised when the shield filter absorbs part of an incoming damage event.
    /// </summary>
    public struct ShieldDeltaEvent : IEvent {
        public EntityGID Target;
        public float Absorbed;
        public float ShieldRemaining;
    }
}
