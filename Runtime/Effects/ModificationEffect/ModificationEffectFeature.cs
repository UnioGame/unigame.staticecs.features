using FFS.Libraries.StaticEcs;

namespace unigame.staticecs.features {
    /// <summary>
    /// Time-bounded characteristic modification slice. Generic over the target characteristic
    /// type; project code instantiates one feature per stat (e.g.
    /// <c>ModificationEffectFeature&lt;Main, SpeedCharacteristic&gt;</c>).
    ///
    /// Requires the matching <see cref="CharacteristicFeature{TWorld, TCharacteristic}"/> and
    /// <see cref="ModifierBackRefFeature{TWorld}"/> to be registered first.
    /// </summary>
    public class ModificationEffectFeature<TWorld, TStat> : EffectFeature<TWorld, ModificationEffect<TStat>>
        where TWorld : struct, IWorldType
        where TStat : struct, ICharacteristicType {
        public ModificationEffectFeature(
            int maxStacks = 1,
            bool refreshOnReapply = true,
            short tickOrder = DefaultTickOrder,
            bool registerTickSystem = true)
            : base(new ModificationEffectHandler<TWorld, TStat>(), maxStacks, refreshOnReapply, tickOrder, registerTickSystem) {
        }

        public override void RegisterTypes(World<TWorld>.TypeRegistrar types) {
            types.Component<ModificationEffectData<TStat>>();
            base.RegisterTypes(types);
        }
    }
}
