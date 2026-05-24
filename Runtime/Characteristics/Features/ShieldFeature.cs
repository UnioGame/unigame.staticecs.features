using FFS.Libraries.StaticEcs;

namespace unigame.staticecs.features {
    public class ShieldFeature<TWorld> : CharacteristicFeature<TWorld, ShieldCharacteristic>
        where TWorld : struct, IWorldType { }
}
