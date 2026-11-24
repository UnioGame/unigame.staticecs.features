namespace UniGame.StaticEcs.Features
{
    using Cysharp.Threading.Tasks;
    using FFS.Libraries.StaticEcs;
    using UniGame.Core.Runtime;

    /// <summary>Marks a world with the shared effects lifecycle infrastructure.</summary>
    public struct EffectsCoreResource : IResource
    {
    }

    /// <summary>Registers shared effect tracking types and registries once per world.</summary>
    public class EffectsCoreFeature<TWorld> : StaticEcsFeature<TWorld>
        where TWorld : struct, IWorldType
    {
        /// <summary>Reserves the source cleanup hook before test assembly scanning.</summary>
        public EffectsCoreFeature()
        {
            if (World<TWorld>.Status != WorldStatus.Created)
                return;

            // Test composition scans the feature assembly after constructing this feature. Reserve the
            // source delete hook first so its component pool is deleted before the target multi pool.
            if (!World<TWorld>.IsComponentTypeRegistered<EffectTrackerComponent>())
                World<TWorld>.Types().Component<EffectTrackerComponent>();
        }

        /// <inheritdoc />
        public override UniTask InitializeAsync(ILifeTime lifeTime)
        {
            var core = new EffectsCoreResource();
            var ids = new EffectIdRegistry();
            var effects = new EffectRegistry();
            var conflicts = new EffectConflictRegistry();

            World<TWorld>.SetResource(core);
            World<TWorld>.SetResource(ids);
            World<TWorld>.SetResource(effects);
            World<TWorld>.SetResource(conflicts);
            return UniTask.CompletedTask;
        }
    }
}
