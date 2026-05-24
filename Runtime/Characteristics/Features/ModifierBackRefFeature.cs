using FFS.Libraries.StaticEcs;
using unigame.staticecs;
using unigame.staticecs.Modifiers;

namespace unigame.staticecs.features {
    public class ModifierBackRefFeature<TWorld> : StaticEcsFeature<TWorld>
        where TWorld : struct, IWorldType {
        public override void RegisterTypes(World<TWorld>.TypeRegistrar types) {
            types
                .Component<ModifierSourceTracker>()
                .Multi<ModifierBackRef>();

            if (!World<TWorld>.HasResource<ModifierRegistry>()) {
                World<TWorld>.SetResource(new ModifierRegistry());
            }
        }
    }
}
