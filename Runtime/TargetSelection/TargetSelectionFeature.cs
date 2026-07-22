namespace UniGame.StaticEcs.Features
{
    using System.Threading;
    using Cysharp.Threading.Tasks;
    using FFS.Libraries.StaticEcs;

    /// <summary>
    /// Registers <see cref="TargetableTag"/>, the default <see cref="KdTreeTargetIndex{TWorld}"/>
    /// resource, and <see cref="TargetIndexRebuildSystem{TWorld}"/>. Replace the index by setting a
    /// custom <c>ITargetIndex&lt;TWorld&gt;</c> resource before this feature's RegisterTypes runs.
    /// </summary>
    public class TargetSelectionFeature<TWorld>
        : StaticEcsFeature<TWorld>,
            IStaticEcsSystemsFeature<TWorld, StaticEcsUpdateSystems>
        where TWorld : struct, IWorldType
    {
        public const short DefaultRebuildOrder = 50;

        private readonly short _rebuildOrder;
        private readonly bool _registerRebuildSystem;

        public TargetSelectionFeature(
            bool registerRebuildSystem = true,
            short rebuildOrder = DefaultRebuildOrder
        )
        {
            _registerRebuildSystem = registerRebuildSystem;
            _rebuildOrder = rebuildOrder;
        }

        public override void RegisterTypes(World<TWorld>.TypeRegistrar types)
        {
            types.Tag<TargetableTag>();

            if (!World<TWorld>.HasResource<ITargetIndex<TWorld>>())
            {
                World<TWorld>.SetResource<ITargetIndex<TWorld>>(new KdTreeTargetIndex<TWorld>());
            }
        }

        public UniTask RegisterSystemsAsync(
            StaticEcsSystemsBuilder<TWorld, StaticEcsUpdateSystems> systems,
            CancellationToken cancellationToken
        )
        {
            if (!_registerRebuildSystem)
            {
                return UniTask.CompletedTask;
            }
            systems.Add(new TargetIndexRebuildSystem<TWorld>(), _rebuildOrder);
            return UniTask.CompletedTask;
        }
    }
}
