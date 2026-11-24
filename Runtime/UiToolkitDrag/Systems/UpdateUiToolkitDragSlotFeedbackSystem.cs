namespace UniGame.StaticEcs.Features
{
    using FFS.Libraries.StaticEcs;

    /// <summary>Colors only the slot currently under an active dragged item.</summary>
    public class UpdateUiToolkitDragSlotFeedbackSystem<TWorld> : ISystem
        where TWorld : struct, IWorldType
    {
        public void Update()
        {
            var hoveredSlot = default(EntityGID);
            var canDrop = false;

            foreach (var item in World<TWorld>
                         .Query<All<DragObserveComponent, DragableComponent,
                             IsDragStateTag, DragTargetComponent>>().Entities())
            {
                var target = item.Read<DragTargetComponent>();
                hoveredSlot = target.Slot;
                canDrop = target.CanDrop;
                break;
            }

            foreach (var slot in World<TWorld>
                         .Query<All<DragSlotComponent, UiToolkitElementComponent>>().Entities())
            {
                var element = slot.Read<UiToolkitElementComponent>().Element;
                if (element == null)
                    continue;
                var hovered = slot.GID == hoveredSlot && hoveredSlot != default;
                element.EnableInClassList("drag-slot--drop-allowed", hovered && canDrop);
                element.EnableInClassList("drag-slot--drop-blocked", hovered && !canDrop);
            }
        }

        public void Destroy()
        {
            foreach (var slot in World<TWorld>
                         .Query<All<DragSlotComponent, UiToolkitElementComponent>>().Entities())
            {
                var element = slot.Read<UiToolkitElementComponent>().Element;
                element?.EnableInClassList("drag-slot--drop-allowed", false);
                element?.EnableInClassList("drag-slot--drop-blocked", false);
            }
        }
    }
}
