namespace UniGame.StaticEcs.Features
{
    using Cysharp.Threading.Tasks;
    using FFS.Libraries.StaticEcs;
    using UniGame.Core.Runtime;
    using UniGame.StaticEcs;
    using UniGame.StaticEcs.Unity;

    /// <summary>Owns generic UI Toolkit drag state and accepted drop events for one ECS world.</summary>
    public class UiToolkitDragFeature<TWorld> : StaticEcsFeature<TWorld>,
        IStaticEcsFeatureTypeRegistrar<TWorld>
        where TWorld : struct, IWorldType
    {
        public void RegisterTypes(World<TWorld>.TypeRegistrar types)
        {
            if (!World<TWorld>.IsComponentTypeRegistered<DragableComponent>())
                types.Component<DragableComponent>();
            if (!World<TWorld>.IsComponentTypeRegistered<DragSlotComponent>())
                types.Component<DragSlotComponent>();
            if (!World<TWorld>.IsComponentTypeRegistered<DragObserveComponent>())
                types.Component<DragObserveComponent>();
            if (!World<TWorld>.IsComponentTypeRegistered<DragSlotMembershipComponent>())
                types.Component<DragSlotMembershipComponent>();
            if (!World<TWorld>.IsComponentTypeRegistered<DragTargetComponent>())
                types.Component<DragTargetComponent>();
            if (!World<TWorld>.IsTagTypeRegistered<IsDragStateTag>())
                types.Tag<IsDragStateTag>();
            if (!World<TWorld>.IsTagTypeRegistered<DragBlockedTag>())
                types.Tag<DragBlockedTag>();
            if (!World<TWorld>.IsComponentTypeRegistered<UiToolkitElementComponent<DragSlotToolkitElement>>())
                types.Component<UiToolkitElementComponent<DragSlotToolkitElement>>();
            if (!World<TWorld>.IsEventTypeRegistered<UiToolkitDragDropEvent>())
                types.Event<UiToolkitDragDropEvent>();
        }

        public override UniTask InitializeAsync(ILifeTime lifeTime)
        {
#if GAME_HEADLESS_CLIENT || UNITY_SERVER
            return UniTask.CompletedTask;
#else
            var input = new UiToolkitDragInputResource<TWorld>();
            World<TWorld>.SetResource(input);
            var visual = new UiToolkitDragVisualResource<TWorld>();
            World<TWorld>.SetResource(visual);
            World<TWorld>.Systems<StaticEcsUpdateSystems>.Add(new PollUiToolkitDragInputSystem<TWorld>(), 10004);
            World<TWorld>.Systems<StaticEcsUpdateSystems>.Add(new ObserveUiToolkitDragSystem<TWorld>(), 10005);
            World<TWorld>.Systems<StaticEcsUpdateSystems>.Add(new UpdateUiToolkitDragSystem<TWorld>(), 10006);
            World<TWorld>.Systems<StaticEcsUpdateSystems>.Add(new ResolveUiToolkitDragTargetSystem<TWorld>(), 10007);
            World<TWorld>.Systems<StaticEcsUpdateSystems>.Add(new CompleteUiToolkitDragSystem<TWorld>(), 10008);
            World<TWorld>.Systems<StaticEcsUpdateSystems>.Add(new RenderUiToolkitDragVisualSystem<TWorld>(), 10009);
            World<TWorld>.Systems<StaticEcsUpdateSystems>.Add(new UpdateUiToolkitDragSlotFeedbackSystem<TWorld>(), 10010);
            return UniTask.CompletedTask;
#endif
        }
    }
}
