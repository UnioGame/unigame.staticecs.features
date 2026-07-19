using FFS.Libraries.StaticEcs;

namespace UniGame.StaticEcs.Features {
    public struct StunChangedEvent : IEvent {
        public EntityGID Target;
        public int PreviousSourceCount;
        public int SourceCount;
        public bool BecameActive;
        public bool BecameInactive;
    }
}
