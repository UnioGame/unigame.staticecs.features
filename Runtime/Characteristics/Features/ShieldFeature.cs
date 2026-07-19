using FFS.Libraries.StaticEcs;

namespace UniGame.StaticEcs.Features {
    public class ShieldFeature<TWorld> : CharacteristicFeature<TWorld, ShieldCharacteristic>
        where TWorld : struct, IWorldType { }
}
