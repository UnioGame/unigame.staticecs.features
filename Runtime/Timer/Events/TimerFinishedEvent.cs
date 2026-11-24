namespace UniGame.StaticEcs.Features
{
    using FFS.Libraries.StaticEcs;

    /// <summary>Reports that a count-up timer reached its optional limit.</summary>
    public struct TimerFinishedEvent : IEvent
    {
        public EntityGID Entity;
    }
}
