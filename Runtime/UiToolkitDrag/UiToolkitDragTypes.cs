namespace UniGame.StaticEcs.Features
{
    using System;
    using FFS.Libraries.StaticEcs;
    using UnityEngine;
    using UnityEngine.UIElements;

    /// <summary>Marks an element as a drag source and describes its accepted slot categories.</summary>
    public struct DragableComponent : IComponent
    {
        public uint Mask;
    }

    /// <summary>Marks a slot as a drop target.</summary>
    public struct DragSlotComponent : IComponent
    {
        public uint AllowedMask;
    }

    /// <summary>Explicit slot ownership for an element occupying that slot.</summary>
    public struct DragSlotMembershipComponent : IComponent
    {
        public EntityGID Slot;
    }

    /// <summary>Drop candidate resolved for the current pointer position.</summary>
    public struct DragTargetComponent : IComponent
    {
        public EntityGID Slot;
        public bool CanDrop;
    }

    /// <summary>Tracks a pressed drag source until release or cancellation.</summary>
    public struct DragObserveComponent : IComponent
    {
        public EntityGID SourceSlot;
        public Vector2 StartScreen;
        public Vector2 StartPanel;
        public Vector2 CurrentScreen;
        public int PointerId;
    }

    /// <summary>Marks a source that has passed the drag distance threshold.</summary>
    public struct IsDragStateTag : ITag
    {
    }

    /// <summary>Blocks pointer initiation while the owning application awaits a result.</summary>
    public struct DragBlockedTag : ITag
    {
    }

    /// <summary>Latest mouse state used by drag systems; runtime polling can be disabled in isolated tests.</summary>
    public sealed class UiToolkitDragInputResource<TWorld> : IResource
        where TWorld : struct, IWorldType
    {
        public Vector2 Screen;
        public bool IsPressed;
        public bool Released;
        public bool Cancelled;
        public bool RuntimePolling = true;
    }

    /// <summary>Reports an accepted UI drop without changing gameplay state.</summary>
    public struct UiToolkitDragDropEvent : IEvent
    {
        public EntityGID SourceElement;
        public EntityGID SourceSlot;
        public EntityGID TargetSlot;
    }

    /// <summary>UXML slot whose entity receives a drag slot converter.</summary>
    [UxmlElement]
    public partial class DragSlotToolkitElement : EcsUiToolkitElement<DragSlotToolkitElement>
    {
        public void SetDropHoverFeedback(bool hovered, bool canDrop)
        {
            EnableInClassList("drag-slot--drop-allowed", hovered && canDrop);
            EnableInClassList("drag-slot--drop-blocked", hovered && !canDrop);
        }
    }

    /// <summary>Applies a drag mask to a UI Toolkit element entity.</summary>
    [Serializable]
    [UxmlObject]
    public partial class DragableElementConverter : UiToolkitElementConverter
    {
        [UxmlAttribute]
        public uint mask = 1;

        public override void Apply<TWorld>(World<TWorld>.Entity entity) =>
            entity.Set(new DragableComponent { Mask = mask });
    }

    /// <summary>Assigns an explicit containing slot to an element entity.</summary>
    [Serializable]
    [UxmlObject]
    public partial class DragSlotMembershipElementConverter : UiToolkitElementConverter
    {
        public EcsUiToolkitElement slot;

        public override void Apply<TWorld>(World<TWorld>.Entity entity)
        {
            if (slot != null && slot.Entity.TryUnpack<TWorld>(out var slotEntity) &&
                slotEntity.Has<DragSlotComponent>())
                entity.Set(new DragSlotMembershipComponent { Slot = slotEntity.GID });
        }
    }

    /// <summary>Applies a slot acceptance mask to a UI Toolkit element entity.</summary>
    [Serializable]
    [UxmlObject]
    public partial class DragSlotElementConverter : UiToolkitElementConverter
    {
        [UxmlAttribute]
        public uint allowedMask = uint.MaxValue;

        public override void Apply<TWorld>(World<TWorld>.Entity entity) =>
            entity.Set(new DragSlotComponent { AllowedMask = allowedMask });
    }

    /// <summary>Pure category matching used by drag source and target validation.</summary>
    public static class UiToolkitDragMask
    {
        public static bool Accepts(uint itemMask, uint allowedMask) =>
            (itemMask & allowedMask) != 0;

    }
}
