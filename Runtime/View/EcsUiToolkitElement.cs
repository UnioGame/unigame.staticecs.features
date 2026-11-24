namespace UniGame.StaticEcs.Features
{
    using System;
    using System.Collections.Generic;
    using FFS.Libraries.StaticEcs;
    using UnityEngine;
    using UnityEngine.UIElements;

    /// <summary>Base for UXML elements that receive an entity while their owning view is active.</summary>
    [UxmlElement(visibility = LibraryVisibility.Hidden)]
    public abstract partial class EcsUiToolkitElement : VisualElement
    {
        private bool _pointerInside;
        private bool _trackingStopped;

        internal static event Action<EcsUiToolkitElement> HierarchyChanged;

        /// <summary>Mouse button transitions, emitted without accessing an ECS world.</summary>
        public static event Action<EcsUiToolkitElement, int, int, Vector2> PointerPressed;
        public static event Action<EcsUiToolkitElement, int, int, Vector2> PointerReleased;

        internal bool CanRegisterEntity => !_trackingStopped;

        protected EcsUiToolkitElement()
        {
            RegisterCallback<AttachToPanelEvent>(OnAttachedToPanel);
            RegisterCallback<PointerEnterEvent>(OnPointerEnter);
            RegisterCallback<PointerLeaveEvent>(OnPointerLeave);
            RegisterCallback<PointerDownEvent>(OnPointerDown);
            RegisterCallback<PointerUpEvent>(OnPointerUp);
            RegisterCallback<DetachFromPanelEvent>(OnDetachedFromPanel);
        }

        [UxmlObjectReference("converters")]
        public List<UiToolkitElementConverter> Converters { get; set; } = new();

        /// <summary>Entity in the owning view's world, or default while unregistered.</summary>
        public EntityGID Entity { get; internal set; }

        /// <summary>Current mouse state, projected to ECS by the pointer feature.</summary>
        public bool IsPointerHovered => _pointerInside;

        /// <summary>Updates the pointer state; UI Toolkit callbacks call this on mouse enter and leave.</summary>
        public void SetPointerInside(bool inside)
        {
            _pointerInside = inside;
        }

        /// <summary>Reports a mouse button press to the pointer feature.</summary>
        protected void ReportPointerPressed(int pointerId, int button, Vector2 position)
        {
            if (!_trackingStopped && pointerId == PointerId.mousePointerId)
                PointerPressed?.Invoke(this, pointerId, button, position);
        }

        /// <summary>Reports a mouse button release to the pointer feature.</summary>
        protected void ReportPointerReleased(int pointerId, int button, Vector2 position)
        {
            if (!_trackingStopped && pointerId == PointerId.mousePointerId)
                PointerReleased?.Invoke(this, pointerId, button, position);
        }

        /// <summary>Requests registration after adding this element to an active view.</summary>
        public void RequestEntityRegistration()
        {
            if (!_trackingStopped)
                HierarchyChanged?.Invoke(this);
        }

        /// <summary>Ends pointer tracking and releases the entity on the next ECS update.</summary>
        protected void StopPointerTracking()
        {
            if (_trackingStopped)
                return;

            _trackingStopped = true;
            SetPointerInside(false);
            HierarchyChanged?.Invoke(this);
            UnregisterCallback<AttachToPanelEvent>(OnAttachedToPanel);
            UnregisterCallback<PointerEnterEvent>(OnPointerEnter);
            UnregisterCallback<PointerLeaveEvent>(OnPointerLeave);
            UnregisterCallback<PointerDownEvent>(OnPointerDown);
            UnregisterCallback<PointerUpEvent>(OnPointerUp);
            UnregisterCallback<DetachFromPanelEvent>(OnDetachedFromPanel);
        }

        private void OnAttachedToPanel(AttachToPanelEvent evt)
        {
            if (ReferenceEquals(evt.target, this))
                RequestEntityRegistration();
        }

        private void OnPointerEnter(PointerEnterEvent evt)
        {
            if (evt.pointerId == PointerId.mousePointerId && ReferenceEquals(evt.target, this))
                SetPointerInside(true);
        }

        private void OnPointerLeave(PointerLeaveEvent evt)
        {
            if (evt.pointerId == PointerId.mousePointerId && ReferenceEquals(evt.target, this))
                SetPointerInside(false);
        }

        private void OnPointerDown(PointerDownEvent evt)
        {
            if (IsClosestEcsElement(evt.target as VisualElement))
                ReportPointerPressed(evt.pointerId, evt.button,
                    new Vector2(evt.position.x, evt.position.y));
        }

        private void OnPointerUp(PointerUpEvent evt)
        {
            if (IsClosestEcsElement(evt.target as VisualElement))
                ReportPointerReleased(evt.pointerId, evt.button,
                    new Vector2(evt.position.x, evt.position.y));
        }

        private bool IsClosestEcsElement(VisualElement target)
        {
            for (var current = target; current != null; current = current.parent)
                if (current is EcsUiToolkitElement element)
                    return ReferenceEquals(element, this);

            return false;
        }

        private void OnDetachedFromPanel(DetachFromPanelEvent evt)
        {
            if (ReferenceEquals(evt.target, this))
            {
                SetPointerInside(false);
                HierarchyChanged?.Invoke(this);
            }
        }

        internal virtual void AddTypeComponent<TWorld>(World<TWorld>.Entity entity)
            where TWorld : struct, IWorldType
        {
        }
    }

    /// <summary>Adds a type-specific marker to the entity of a UI Toolkit element.</summary>
    [UxmlElement(visibility = LibraryVisibility.Hidden)]
    public abstract partial class EcsUiToolkitElement<TElement> : EcsUiToolkitElement
        where TElement : EcsUiToolkitElement<TElement>
    {
        internal sealed override void AddTypeComponent<TWorld>(World<TWorld>.Entity entity)
        {
            entity.Set(new UiToolkitElementComponent<TElement>());
        }
    }
}
