namespace UniGame.StaticEcs.Features
{
    using Unity;
    using UnityEngine;

    [AddComponentMenu("Static ECS/Characteristics/Speed Converter")]
    public sealed class SpeedConverter : CharacteristicConverter<Main, SpeedCharacteristic> { }

    /// <summary>Inline Main-world speed characteristic converter.</summary>
    [System.Serializable]
    public sealed class SpeedSerializableConverter
        : CharacteristicSerializableConverter<SpeedCharacteristic> { }
}
