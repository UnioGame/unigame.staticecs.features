using FFS.Libraries.StaticEcs;

namespace UniGame.StaticEcs.Features {
    public class HealthFeature<TWorld> : CharacteristicFeature<TWorld, HealthCharacteristic>
        where TWorld : struct, IWorldType { }
}
