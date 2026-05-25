using FFS.Libraries.StaticEcs;
using unigame.staticecs;

namespace unigame.staticecs.features {
    /// <summary>
    /// Registers <see cref="TargetableTag"/>, the default <see cref="KdTreeTargetIndex{TWorld}"/>
    /// resource, and <see cref="TargetIndexRebuildSystem{TWorld}"/>. Replace the index by setting a
    /// custom <c>ITargetIndex&lt;TWorld&gt;</c> resource before this feature's RegisterTypes runs.
    /// </summary>
    public class TargetSelectionFeature<TWorld> :
        StaticEcsFeature<TWorld>,
        IStaticEcsSystemsFeature<TWorld, StaticEcsUpdateSystems>
        where TWorld : struct, IWorldType {
        public const short DefaultRebuildOrder = 50;

        private readonly short _rebuildOrder;
        private readonly bool _registerRebuildSystem;

        public TargetSelectionFeature(
            bool registerRebuildSystem = true,
            short rebuildOrder = DefaultRebuildOrder) {
            _registerRebuildSystem = registerRebuildSystem;
            _rebuildOrder = rebuildOrder;
        }

        public override void RegisterTypes(World<TWorld>.TypeRegistrar types) {
            types.Tag<TargetableTag>();

            if (!World<TWorld>.HasResource<ITargetIndex<TWorld>>()) {
                World<TWorld>.SetResource<ITargetIndex<TWorld>>(new KdTreeTargetIndex<TWorld>());
            }
        }

        public void RegisterSystems(StaticEcsSystemsBuilder<TWorld, StaticEcsUpdateSystems> systems) {
            if (!_registerRebuildSystem) {
                return;
            }
            systems.Add(new TargetIndexRebuildSystem<TWorld>(), _rebuildOrder);
        }
    }
}
