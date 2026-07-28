namespace UniGame.StaticEcs.Features
{
    /// <summary>Describes the ECS-visible lifecycle state of a view.</summary>
    public enum ViewLifecycleStatus : byte
    {
        /// <summary>Waiting for an earlier request in the same FIFO queue.</summary>
        Queued = 0,

        /// <summary>View System is creating or initializing the view.</summary>
        Opening = 1,

        /// <summary>The view exists and is hidden.</summary>
        Hidden = 2,

        /// <summary>The view is transitioning to visible.</summary>
        Showing = 3,

        /// <summary>The view is visible.</summary>
        Shown = 4,

        /// <summary>The view is transitioning to hidden.</summary>
        Hiding = 5,

        /// <summary>The view is closing.</summary>
        Closing = 6,

        /// <summary>The view is closed and awaiting ECS cleanup.</summary>
        Closed = 7,

        /// <summary>Creation or binding failed.</summary>
        Failed = 8
    }
}
