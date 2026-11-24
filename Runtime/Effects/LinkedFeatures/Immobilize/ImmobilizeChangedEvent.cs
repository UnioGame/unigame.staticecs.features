namespace UniGame.StaticEcs.Features
{
    using FFS.Libraries.StaticEcs;

    public struct ImmobilizeChangedEvent : IEvent
    {
        public EntityGID Target;
        public int PreviousSourceCount;
        public int SourceCount;
        public bool BecameActive;
        public bool BecameInactive;
    }
}
