using FFS.Libraries.StaticEcs;

namespace unigame.staticecs.features {
    public struct StunChangedEvent : IEvent {
        public EntityGID Target;
        public int PreviousSourceCount;
        public int SourceCount;
        public bool BecameActive;
        public bool BecameInactive;
    }
}
