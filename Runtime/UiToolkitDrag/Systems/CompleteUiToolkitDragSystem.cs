namespace UniGame.StaticEcs.Features
{
    using System.Collections.Generic;
    using FFS.Libraries.StaticEcs;
    using UnityEngine.UIElements;

    /// <summary>Validates a released drop and clears the temporary drag state.</summary>
    public class CompleteUiToolkitDragSystem<TWorld> : ISystem
        where TWorld : struct, IWorldType
    {
        private EventReceiver<TWorld, UiToolkitPointerButtonEvent> _receiver;
        private readonly HashSet<int> _releasedPointers = new();

        public void Init() =>
            _receiver = World<TWorld>.RegisterEventReceiver<UiToolkitPointerButtonEvent>();

        public void Update()
        {
            var input = World<TWorld>.GetResource<UiToolkitDragInputResource<TWorld>>();
            _releasedPointers.Clear();
            foreach (var message in _receiver)
                if (!message.Value.IsPressed && message.Value.Button == 0)
                    _releasedPointers.Add(message.Value.PointerId);

            foreach (var item in World<TWorld>.Query<All<DragObserveComponent,
                         DragableComponent>>().Entities())
            {
                var observe = item.Read<DragObserveComponent>();
                var mouseReleased = observe.PointerId == PointerId.mousePointerId &&
                                    (input.Released || !input.IsPressed);
                if (!input.Cancelled && !mouseReleased &&
                    !_releasedPointers.Contains(observe.PointerId))
                    continue;

                if (!input.Cancelled && item.Has<IsDragStateTag>() &&
                    item.Has<DragTargetComponent>())
                {
                    var target = item.Read<DragTargetComponent>();
                    if (target.CanDrop)
                    {
                        World<TWorld>.SendEvent(new UiToolkitDragDropEvent
                        {
                            SourceElement = item.GID,
                            SourceSlot = observe.SourceSlot,
                            TargetSlot = target.Slot
                        });
                    }
                }

                item.Delete<DragObserveComponent>();
                if (item.Has<IsDragStateTag>())
                    item.Delete<IsDragStateTag>();
                if (item.Has<DragTargetComponent>())
                    item.Delete<DragTargetComponent>();
            }
        }

        public void Destroy()
        {
            World<TWorld>.DeleteEventReceiver(ref _receiver);
            _releasedPointers.Clear();
        }
    }
}
