namespace UniGame.StaticEcs.Features
{
    using FFS.Libraries.StaticEcs;

    public class SpeedFeature<TWorld> : CharacteristicFeature<TWorld, SpeedCharacteristic>
        where TWorld : struct, IWorldType { }
}
