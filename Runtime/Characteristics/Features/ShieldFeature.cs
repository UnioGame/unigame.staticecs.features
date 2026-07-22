namespace UniGame.StaticEcs.Features
{
    using FFS.Libraries.StaticEcs;

    public class ShieldFeature<TWorld> : CharacteristicFeature<TWorld, ShieldCharacteristic>
        where TWorld : struct, IWorldType { }
}
