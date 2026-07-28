namespace UniGame.StaticEcs.Features
{
    using System;
    using FFS.Libraries.StaticEcs;
    using UnityEngine;
    using ViewType = UniModules.UniGame.UiSystem.Runtime.ViewType;

    /// <summary>Defines how View System places a created or existing view.</summary>
    [Serializable]
    public struct ViewPlacement
    {
        /// <summary>Parent selection strategy.</summary>
        public ViewPlacementMode Mode;

        /// <summary>Built-in or custom layout identifier.</summary>
        public string Layout;

        /// <summary>Named container identifier.</summary>
        public string Container;

        /// <summary>Parent ECS-managed view.</summary>
        public ViewKey ParentView;

        /// <summary>Entity that supplies a parent transform.</summary>
        public EntityGID ParentEntity;

        /// <summary>Explicit parent transform.</summary>
        public Transform Parent;

        /// <summary>Optional position override.</summary>
        public Vector3 Position;

        /// <summary>Optional rotation override.</summary>
        public Quaternion Rotation;

        /// <summary>Optional scale override.</summary>
        public Vector3 Scale;

        /// <summary>Coordinate space for placement overrides.</summary>
        public ViewPlacementSpace Space;

        /// <summary>Whether to apply the position override.</summary>
        public bool ApplyPosition;

        /// <summary>Whether to apply the rotation override.</summary>
        public bool ApplyRotation;

        /// <summary>Whether to apply the scale override.</summary>
        public bool ApplyScale;

        /// <summary>Whether reparenting preserves world coordinates.</summary>
        public bool StayWorldPosition;

        /// <summary>Whether a full named container may be reused.</summary>
        public bool UseBusyContainer;

        /// <summary>Creates a placement in a built-in View System layout.</summary>
        public static ViewPlacement InLayout(ViewType layout)
        {
            if (layout == ViewType.None)
                throw new ArgumentOutOfRangeException(nameof(layout), layout, "ViewType.None is not a layout.");

            return InLayout(layout.ToString());
        }

        /// <summary>Creates a placement in a built-in or custom View System layout.</summary>
        public static ViewPlacement InLayout(string layout)
        {
            if (string.IsNullOrWhiteSpace(layout))
                throw new ArgumentException("Layout must be specified.", nameof(layout));

            return new ViewPlacement
            {
                Mode = ViewPlacementMode.Layout,
                Layout = layout,
                Scale = Vector3.one,
                Rotation = Quaternion.identity,
                Space = ViewPlacementSpace.Prefab
            };
        }

        /// <summary>Creates a placement in a named container.</summary>
        public static ViewPlacement InContainer(string container, bool useBusyContainer = false)
        {
            if (string.IsNullOrWhiteSpace(container))
                throw new ArgumentException("Container must be specified.", nameof(container));

            return new ViewPlacement
            {
                Mode = ViewPlacementMode.Container,
                Container = container,
                UseBusyContainer = useBusyContainer,
                Scale = Vector3.one,
                Rotation = Quaternion.identity,
                Space = ViewPlacementSpace.Prefab
            };
        }

        /// <summary>Creates a placement below another ECS-managed view.</summary>
        public static ViewPlacement UnderView(ViewKey parentView, bool stayWorldPosition = false)
        {
            return new ViewPlacement
            {
                Mode = ViewPlacementMode.ParentView,
                ParentView = parentView,
                StayWorldPosition = stayWorldPosition,
                Scale = Vector3.one,
                Rotation = Quaternion.identity,
                Space = ViewPlacementSpace.Prefab
            };
        }

        /// <summary>Creates a placement below the transform represented by an ECS entity.</summary>
        public static ViewPlacement UnderEntity(EntityGID parentEntity, bool stayWorldPosition = false)
        {
            return new ViewPlacement
            {
                Mode = ViewPlacementMode.ParentEntity,
                ParentEntity = parentEntity,
                StayWorldPosition = stayWorldPosition,
                Scale = Vector3.one,
                Rotation = Quaternion.identity,
                Space = ViewPlacementSpace.Prefab
            };
        }

        /// <summary>Creates a placement below an explicit Unity transform.</summary>
        public static ViewPlacement UnderTransform(Transform parent, bool stayWorldPosition = false)
        {
            return new ViewPlacement
            {
                Mode = ViewPlacementMode.Transform,
                Parent = parent,
                StayWorldPosition = stayWorldPosition,
                Scale = Vector3.one,
                Rotation = Quaternion.identity,
                Space = ViewPlacementSpace.Prefab
            };
        }

        /// <summary>Marks a view that already exists on an entity prefab.</summary>
        public static ViewPlacement Existing()
        {
            return new ViewPlacement
            {
                Mode = ViewPlacementMode.Existing,
                Scale = Vector3.one,
                Rotation = Quaternion.identity,
                Space = ViewPlacementSpace.Prefab
            };
        }
    }

    /// <summary>Defines the parent selection strategy for a view.</summary>
    public enum ViewPlacementMode : byte
    {
        /// <summary>Place the view in a View System layout.</summary>
        Layout = 0,

        /// <summary>Place the view in a named container.</summary>
        Container = 1,

        /// <summary>Place the view under another ECS-managed view.</summary>
        ParentView = 2,

        /// <summary>Place the view under an entity transform.</summary>
        ParentEntity = 3,

        /// <summary>Place the view under an explicit Unity transform.</summary>
        Transform = 4,

        /// <summary>Bind a view already present on an entity prefab.</summary>
        Existing = 5
    }

    /// <summary>Defines the coordinate space used by optional placement overrides.</summary>
    public enum ViewPlacementSpace : byte
    {
        /// <summary>Retain the prefab-authored transform values.</summary>
        Prefab = 0,

        /// <summary>Apply overrides in local space.</summary>
        Local = 1,

        /// <summary>Apply overrides in world space.</summary>
        World = 2
    }
}
