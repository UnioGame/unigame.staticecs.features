namespace UniGame.StaticEcs.Features
{
    using FFS.Libraries.StaticEcs;

    public class HealthFeature<TWorld> : CharacteristicFeature<TWorld, HealthCharacteristic>
        where TWorld : struct, IWorldType { }
}
