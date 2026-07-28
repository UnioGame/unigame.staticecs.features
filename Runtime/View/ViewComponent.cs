namespace UniGame.StaticEcs.Features
{
    using FFS.Libraries.StaticEcs;

    /// <summary>Stores backend-free ECS state for one accepted view request.</summary>
    public struct ViewComponent : IComponent
    {
        /// <summary>Stable view request key.</summary>
        public ViewKey Key;

        /// <summary>View System identifier.</summary>
        public string ViewId;

        /// <summary>Resolved layout identifier.</summary>
        public string Layout;

        /// <summary>Resolved named container.</summary>
        public string Container;

        /// <summary>FIFO queue identifier.</summary>
        public string Queue;

        /// <summary>Entity controlling view lifetime.</summary>
        public EntityGID Owner;

        /// <summary>Current lifecycle state.</summary>
        public ViewLifecycleStatus Status;

        /// <summary>Sibling order.</summary>
        public int Order;
    }
}
