namespace UniGame.StaticEcs.Features
{
    using FFS.Libraries.StaticEcs;
    using UnityEngine;
    using UnityEngine.UIElements;

    /// <summary>Finds the hovered slot and evaluates occupancy from ECS membership.</summary>
    public class ResolveUiToolkitDragTargetSystem<TWorld> : ISystem
        where TWorld : struct, IWorldType
    {
        public void Update()
        {
            var input = World<TWorld>.GetResource<UiToolkitDragInputResource<TWorld>>();
            var registry = World<TWorld>.HasResource<UiToolkitElementRegistryResource<TWorld>>()
                ? World<TWorld>.GetResource<UiToolkitElementRegistryResource<TWorld>>()
                : null;
            foreach (var item in World<TWorld>
                         .Query<All<DragObserveComponent, DragableComponent, IsDragStateTag>>().Entities())
            {
                var targetId = default(EntityGID);
                var bestOrder = float.NegativeInfinity;
                var pointOnScreen = new Vector2(input.Screen.x, Screen.height - input.Screen.y);
                foreach (var slot in World<TWorld>
                             .Query<All<DragSlotComponent, UiToolkitElementComponent>>().Entities())
                {
                    var element = slot.Read<UiToolkitElementComponent>().Element;
                    if (element?.panel == null)
                        continue;

                    if (registry == null ||
                        !registry.TryGetPanelSortingOrder(element.panel, out var panelOrder))
                        continue;

                    var point = RuntimePanelUtils.ScreenToPanel(element.panel, pointOnScreen);
                    var hit = element.panel.Pick(point);
                    for (var current = hit; current != null; current = current.parent)
                    {
                        if (!ReferenceEquals(current, element))
                            continue;
                        if (panelOrder >= bestOrder)
                        {
                            targetId = slot.GID;
                            bestOrder = panelOrder;
                        }
                        break;
                    }
                }

                if (targetId == default ||
                    !targetId.TryUnpack<TWorld>(out var targetSlot) ||
                    !item.Read<DragObserveComponent>().SourceSlot.TryUnpack<TWorld>(out var sourceSlot))
                {
                    if (item.Has<DragTargetComponent>())
                        item.Delete<DragTargetComponent>();
                    continue;
                }

                item.Set(new DragTargetComponent
                {
                    Slot = targetId,
                    CanDrop = CanDrop(item, sourceSlot, targetSlot)
                });
            }
        }

        public bool CanDrop(World<TWorld>.Entity item, World<TWorld>.Entity sourceSlot,
            World<TWorld>.Entity targetSlot)
        {
            var canDrop = sourceSlot.GID != targetSlot.GID &&
                              !item.Has<DragBlockedTag>() &&
                              sourceSlot.Has<DragSlotComponent>() &&
                              targetSlot.Has<DragSlotComponent>() &&
                              !sourceSlot.Has<DragBlockedTag>() &&
                              !targetSlot.Has<DragBlockedTag>() &&
                              UiToolkitDragMask.Accepts(item.Read<DragableComponent>().Mask,
                                  targetSlot.Read<DragSlotComponent>().AllowedMask);
            if (!canDrop)
                return false;

            var occupants = 0;
            foreach (var occupant in World<TWorld>
                         .Query<All<DragSlotMembershipComponent>>().Entities())
            {
                if (occupant.GID == item.GID ||
                    occupant.Read<DragSlotMembershipComponent>().Slot != targetSlot.GID)
                    continue;
                occupants++;
                if (occupants > 1 || !occupant.Has<DragableComponent>() ||
                    occupant.Has<DragBlockedTag>() ||
                    !UiToolkitDragMask.Accepts(occupant.Read<DragableComponent>().Mask,
                        sourceSlot.Read<DragSlotComponent>().AllowedMask))
                    return false;
            }

            return true;
        }
    }
}
