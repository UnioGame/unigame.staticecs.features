namespace UniGame.StaticEcs.Features
{
    using System;
    using Unity;

    /// <summary>Main-world inline converter for one characteristic component.</summary>
    [Serializable]
    public class CharacteristicSerializableConverter<TCharacteristic>
        : CharacteristicSerializableConverter<Main, TCharacteristic>
        where TCharacteristic : struct, ICharacteristicType { }
}
