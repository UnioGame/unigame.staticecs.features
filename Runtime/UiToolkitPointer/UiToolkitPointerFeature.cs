namespace UniGame.StaticEcs.Features
{
    using Cysharp.Threading.Tasks;
    using FFS.Libraries.StaticEcs;
    using UniGame.Core.Runtime;
    using UniGame.StaticEcs.Unity;

    /// <summary>Mirrors UI Toolkit mouse hover and publishes button transitions in one ECS world.</summary>
    public class UiToolkitPointerFeature<TWorld> : StaticEcsFeature<TWorld>
        where TWorld : struct, IWorldType
    {
        public const short UpdateOrder = 10003;

        public override UniTask InitializeAsync(ILifeTime lifeTime)
        {
            World<TWorld>.Systems<StaticEcsUpdateSystems>.Add(
                new UpdateHoveredElementsSystem<TWorld>(), UpdateOrder);
            World<TWorld>.Systems<StaticEcsUpdateSystems>.Add(
                new PublishUiToolkitPointerEventsSystem<TWorld>(), UpdateOrder);
            return UniTask.CompletedTask;
        }
    }
}
