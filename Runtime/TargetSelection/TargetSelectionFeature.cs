namespace UniGame.StaticEcs.Features
{
    using Cysharp.Threading.Tasks;
    using FFS.Libraries.StaticEcs;
    using UniGame.Core.Runtime;

    /// <summary>
    /// Registers <see cref="TargetableTag"/>, the default <see cref="KdTreeTargetIndex{TWorld}"/>
    /// resource, and <see cref="TargetIndexRebuildSystem{TWorld}"/>. Replace the index by setting a
    /// custom <c>ITargetIndex&lt;TWorld&gt;</c> resource before this feature initializes.
    /// </summary>
    public class TargetSelectionFeature<TWorld> : StaticEcsFeature<TWorld>
        where TWorld : struct, IWorldType
    {
        public const short DefaultRebuildOrder = 50;

        /// <summary>Whether the index rebuild system is installed.</summary>
        public bool registerRebuildSystem = true;

        /// <summary>Execution order of index rebuilding.</summary>
        public short rebuildOrder = DefaultRebuildOrder;

        /// <inheritdoc />
        public override UniTask InitializeAsync(ILifeTime lifeTime)
        {
            if (!World<TWorld>.HasResource<TargetSelectionConfig>())
            {
                var configuration = new TargetSelectionConfig
                {
                    RegisterRebuildSystem = registerRebuildSystem,
                    RebuildOrder = rebuildOrder,
                };

                World<TWorld>.SetResource(configuration);
            }

            if (!World<TWorld>.HasResource<ITargetIndex<TWorld>>())
            {
                ITargetIndex<TWorld> index = new KdTreeTargetIndex<TWorld>();
                World<TWorld>.SetResource(index);
            }

            ref var config = ref World<TWorld>.GetResource<TargetSelectionConfig>();
            if (!config.RegisterRebuildSystem)
                return UniTask.CompletedTask;
            World<TWorld>.Systems<StaticEcsUpdateSystems>.Add(
                new TargetIndexRebuildSystem<TWorld>(),
                config.RebuildOrder);
            return UniTask.CompletedTask;
        }
    }

    /// <summary>Controls target-index system composition.</summary>
    public sealed class TargetSelectionConfig : IResource
    {
        /// <summary>Whether the index rebuild system is installed.</summary>
        public bool RegisterRebuildSystem = true;

        /// <summary>Execution order of index rebuilding.</summary>
        public short RebuildOrder = 50;
    }
}
