using FFS.Libraries.StaticEcs;

namespace unigame.staticecs.features {
    public class SpeedFeature<TWorld> : CharacteristicFeature<TWorld, SpeedCharacteristic>
        where TWorld : struct, IWorldType { }
}
