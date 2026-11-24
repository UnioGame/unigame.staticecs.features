namespace UniGame.StaticEcs.Features
{
    using FFS.Libraries.StaticEcs;

    public struct StunChangedEvent : IEvent
    {
        public EntityGID Target;
        public int PreviousSourceCount;
        public int SourceCount;
        public bool BecameActive;
        public bool BecameInactive;
    }
}
