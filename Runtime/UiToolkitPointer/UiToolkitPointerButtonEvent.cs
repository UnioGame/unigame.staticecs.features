namespace UniGame.StaticEcs.Features
{
    using FFS.Libraries.StaticEcs;
    using UnityEngine;

    /// <summary>One mouse button transition on an ECS-backed UI Toolkit element.</summary>
    public struct UiToolkitPointerButtonEvent : IEvent
    {
        public EntityGID Element;
        public bool IsPressed;
        public int PointerId;
        public int Button;

        /// <summary>Pointer position in the element's UI Toolkit panel coordinates.</summary>
        public Vector2 Position;
    }
}
