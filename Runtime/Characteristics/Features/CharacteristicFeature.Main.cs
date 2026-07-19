namespace UniGame.StaticEcs.Features {
    using Unity;

    public class CharacteristicFeature<TCharacteristic> : CharacteristicFeature<Main, TCharacteristic>
        where TCharacteristic : struct, ICharacteristicType { }
}
