namespace UniGame.StaticEcs.Features
{
    using Cysharp.Threading.Tasks;
    using FFS.Libraries.StaticEcs;
    using Modifiers;
    using UniGame.Core.Runtime;

    public class ModifierBackRefFeature<TWorld> : StaticEcsFeature<TWorld>
        where TWorld : struct, IWorldType
    {
        /// <summary>Creates the modifier back-reference feature.</summary>
        public ModifierBackRefFeature()
        {
            if (World<TWorld>.Status != WorldStatus.Created)
                return;

            // Test composition scans the feature assembly after constructing this feature. Reserve the
            // source delete hook first so its component pool is deleted before the target multi pool.
            if (!World<TWorld>.IsComponentTypeRegistered<ModifierTrackerComponent>())
                World<TWorld>.Types().Component<ModifierTrackerComponent>();
        }

        /// <inheritdoc />
        public override UniTask InitializeAsync(ILifeTime lifeTime)
        {
            if (!World<TWorld>.HasResource<ModifierRegistry>())
            {
                var registry = new ModifierRegistry();
                World<TWorld>.SetResource(registry);
            }

            return UniTask.CompletedTask;
        }
    }
}
