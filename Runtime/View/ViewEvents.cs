namespace UniGame.StaticEcs.Features
{
    using FFS.Libraries.StaticEcs;
    using UniGame.ViewSystem.Runtime;

    /// <summary>Requests creation of a view on a specific ECS entity.</summary>
    public struct OpenViewOnEntityEvent : IEvent
    {
        /// <summary>Request key.</summary>
        public ViewKey Key;

        /// <summary>Target entity.</summary>
        public EntityGID Entity;

        /// <summary>View creation data.</summary>
        public ViewRequest Request;
    }

    /// <summary>Requests creation of a dedicated ECS view entity.</summary>
    public struct OpenViewEvent : IEvent
    {
        /// <summary>Request key.</summary>
        public ViewKey Key;

        /// <summary>View creation data.</summary>
        public ViewRequest Request;
    }

    /// <summary>Requests binding of an existing prefab view to an ECS entity.</summary>
    public struct BindExistingViewEvent : IEvent
    {
        /// <summary>Request key.</summary>
        public ViewKey Key;

        /// <summary>Target entity.</summary>
        public EntityGID Entity;

        /// <summary>View binding data.</summary>
        public ViewRequest Request;

        /// <summary>Existing View System view.</summary>
        public IView View;
    }

    /// <summary>Requests a visibility transition for one view.</summary>
    public struct SetViewVisibilityEvent : IEvent
    {
        /// <summary>View key.</summary>
        public ViewKey Key;

        /// <summary>Requested visibility.</summary>
        public bool Visible;
    }

    /// <summary>Requests closing of one view.</summary>
    public struct CloseViewEvent : IEvent
    {
        /// <summary>View key.</summary>
        public ViewKey Key;
    }

    /// <summary>Requests closing views matching the supplied filter.</summary>
    public struct CloseViewsEvent : IEvent
    {
        /// <summary>Selection criteria.</summary>
        public ViewCloseFilter Filter;
    }

    /// <summary>Notifies that a view and its model were attached to ECS.</summary>
    public struct ViewOpenedEvent : IEvent
    {
        /// <summary>View key.</summary>
        public ViewKey Key;

        /// <summary>ECS view entity.</summary>
        public EntityGID Entity;
    }

    /// <summary>Notifies that a view lifecycle state changed.</summary>
    public struct ViewStatusChangedEvent : IEvent
    {
        /// <summary>View key.</summary>
        public ViewKey Key;

        /// <summary>ECS view entity.</summary>
        public EntityGID Entity;

        /// <summary>Current lifecycle state.</summary>
        public ViewLifecycleStatus Status;
    }

    /// <summary>Notifies that a view was closed.</summary>
    public struct ViewClosedEvent : IEvent
    {
        /// <summary>View key.</summary>
        public ViewKey Key;

        /// <summary>ECS view entity.</summary>
        public EntityGID Entity;
    }

    /// <summary>Notifies that a view could not be opened.</summary>
    public struct ViewOpenFailedEvent : IEvent
    {
        /// <summary>View key.</summary>
        public ViewKey Key;

        /// <summary>ECS view entity, if it was allocated.</summary>
        public EntityGID Entity;

        /// <summary>Failure description.</summary>
        public string Reason;
    }
}
