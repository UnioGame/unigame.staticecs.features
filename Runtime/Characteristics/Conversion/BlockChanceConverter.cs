namespace UniGame.StaticEcs.Features
{
    using Unity;
    using UnityEngine;

    [AddComponentMenu("Static ECS/Characteristics/Block Chance Converter")]
    public sealed class BlockChanceConverter
        : CharacteristicConverter<Main, BlockChanceCharacteristic> { }

    /// <summary>Inline Main-world block chance characteristic converter.</summary>
    [System.Serializable]
    public sealed class BlockChanceSerializableConverter
        : CharacteristicSerializableConverter<BlockChanceCharacteristic> { }
}
