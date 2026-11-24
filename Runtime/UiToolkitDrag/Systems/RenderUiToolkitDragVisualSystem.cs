namespace UniGame.StaticEcs.Features
{
    using FFS.Libraries.StaticEcs;
    using UnityEngine;
    using UnityEngine.UIElements;

    /// <summary>Displays the active drag preview near the pointer.</summary>
    public class RenderUiToolkitDragVisualSystem<TWorld> : ISystem
        where TWorld : struct, IWorldType
    {
        public void Update()
        {
            var visual = World<TWorld>.GetResource<UiToolkitDragVisualResource<TWorld>>();
            var input = World<TWorld>.GetResource<UiToolkitDragInputResource<TWorld>>();
            var registry = World<TWorld>.HasResource<UiToolkitElementRegistryResource<TWorld>>()
                ? World<TWorld>.GetResource<UiToolkitElementRegistryResource<TWorld>>()
                : null;
            foreach (var item in World<TWorld>
                         .Query<All<DragObserveComponent, IsDragStateTag,
                             UiToolkitElementComponent>>().Entities())
            {
                var source = item.Read<UiToolkitElementComponent>().Element;
                IPanel targetPanel = null;
                if (item.Has<DragTargetComponent>() &&
                    item.Read<DragTargetComponent>().Slot.TryUnpack<TWorld>(out var targetSlot) &&
                    targetSlot.Has<UiToolkitElementComponent>())
                    targetPanel = targetSlot.Read<UiToolkitElementComponent>().Element?.panel;

                if (targetPanel == null && registry != null)
                {
                    var pointOnScreen = new Vector2(input.Screen.x, Screen.height - input.Screen.y);
                    registry.TryGetTopPanelAt(pointOnScreen, out targetPanel);
                }

                visual.Show(source, input.Screen, targetPanel);
                return;
            }

            visual.Hide();
        }

        public void Destroy()
        {
            if (World<TWorld>.HasResource<UiToolkitDragVisualResource<TWorld>>())
                World<TWorld>.GetResource<UiToolkitDragVisualResource<TWorld>>().Hide();
        }
    }
}
