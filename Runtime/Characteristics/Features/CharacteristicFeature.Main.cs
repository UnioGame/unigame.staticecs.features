using unigame.staticecs.unity;

namespace unigame.staticecs.features {
    public class CharacteristicFeature<TCharacteristic> : CharacteristicFeature<Main, TCharacteristic>
        where TCharacteristic : struct, ICharacteristicType { }
}
