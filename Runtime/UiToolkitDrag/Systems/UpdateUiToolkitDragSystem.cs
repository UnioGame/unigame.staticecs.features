namespace UniGame.StaticEcs.Features
{
    using FFS.Libraries.StaticEcs;
    using UnityEngine;
    using UnityEngine.UIElements;

    /// <summary>Promotes an observed pointer to a drag after the distance threshold.</summary>
    public class UpdateUiToolkitDragSystem<TWorld> : ISystem
        where TWorld : struct, IWorldType
    {
        public void Update()
        {
            var input = World<TWorld>.GetResource<UiToolkitDragInputResource<TWorld>>();
            foreach (var item in World<TWorld>.Query<All<DragObserveComponent,
                         UiToolkitElementComponent>>().Entities())
            {
                ref var observe = ref item.Mut<DragObserveComponent>();
                observe.CurrentScreen = input.Screen;
                var displacement = observe.CurrentScreen - observe.StartScreen;
                var sourceElement = item.Read<UiToolkitElementComponent>().Element;
                if (sourceElement?.panel != null)
                {
                    var panel = sourceElement.panel;
                    var topLeft = new Vector2(input.Screen.x, Screen.height - input.Screen.y);
                    var currentPanel = RuntimePanelUtils.ScreenToPanel(panel, topLeft);
                    var zero = RuntimePanelUtils.ScreenToPanel(panel, Vector2.zero);
                    var one = RuntimePanelUtils.ScreenToPanel(panel, Vector2.one);
                    var panelScale = one - zero;
                    if (Mathf.Abs(panelScale.x) > Mathf.Epsilon &&
                        Mathf.Abs(panelScale.y) > Mathf.Epsilon)
                        displacement = new Vector2(
                            (currentPanel.x - observe.StartPanel.x) / panelScale.x,
                            (currentPanel.y - observe.StartPanel.y) / panelScale.y);
                }
                if (!item.Has<IsDragStateTag>() &&
                    displacement.sqrMagnitude >= 64f)
                    item.Set<IsDragStateTag>();
            }
        }
    }
}
