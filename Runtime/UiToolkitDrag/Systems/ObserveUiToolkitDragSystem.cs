namespace UniGame.StaticEcs.Features
{
    using FFS.Libraries.StaticEcs;

    /// <summary>Starts observing a pressed draggable element and records its source slot.</summary>
    public class ObserveUiToolkitDragSystem<TWorld> : ISystem
        where TWorld : struct, IWorldType
    {
        private EventReceiver<TWorld, UiToolkitPointerButtonEvent> _receiver;

        public void Init() =>
            _receiver = World<TWorld>.RegisterEventReceiver<UiToolkitPointerButtonEvent>();

        public void Update()
        {
            var input = World<TWorld>.GetResource<UiToolkitDragInputResource<TWorld>>();
            foreach (var message in _receiver)
            {
                var pointer = message.Value;
                if (!pointer.IsPressed || pointer.Button != 0 ||
                    !pointer.Element.TryUnpack<TWorld>(out var item) ||
                    !item.Has<DragableComponent>() ||
                    item.Has<DragBlockedTag>() ||
                    !item.Has<UiToolkitElementComponent>() ||
                    item.Has<DragObserveComponent>())
                    continue;

                var busy = false;
                foreach (var observed in World<TWorld>.Query<All<DragObserveComponent>>().Entities())
                    busy = true;
                if (busy)
                    continue;

                if (!item.Has<DragSlotMembershipComponent>() ||
                    !item.Read<DragSlotMembershipComponent>().Slot.TryUnpack<TWorld>(out var slotEntity) ||
                    !slotEntity.Has<DragSlotComponent>() ||
                    slotEntity.Has<DragBlockedTag>())
                    continue;

                item.Set(new DragObserveComponent
                {
                    SourceSlot = slotEntity.GID,
                    StartScreen = input.Screen,
                    StartPanel = pointer.Position,
                    CurrentScreen = input.Screen,
                    PointerId = pointer.PointerId
                });
            }
        }

        public void Destroy() => World<TWorld>.DeleteEventReceiver(ref _receiver);
    }
}
