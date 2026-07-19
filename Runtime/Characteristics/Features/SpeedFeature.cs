using FFS.Libraries.StaticEcs;

namespace UniGame.StaticEcs.Features {
    public class SpeedFeature<TWorld> : CharacteristicFeature<TWorld, SpeedCharacteristic>
        where TWorld : struct, IWorldType { }
}
