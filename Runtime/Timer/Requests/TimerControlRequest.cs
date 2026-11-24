namespace UniGame.StaticEcs.Features
{
    using FFS.Libraries.StaticEcs;

    /// <summary>Chooses one action for a count-up timer.</summary>
    public enum TimerAction : byte
    {
        None,
        Start,
        Pause,
        Reset,
        Restart
    }

    /// <summary>Applies a timer action during the next update.</summary>
    public struct TimerControlRequest : IEvent
    {
        public EntityGID Target;
        public TimerAction Action;
    }
}
