namespace UniGame.StaticEcs.Features
{
    using Unity;
    using UnityEngine;

    [AddComponentMenu("Static ECS/Characteristics/Health Converter")]
    public sealed class HealthConverter : CharacteristicConverter<Main, HealthCharacteristic> { }

    /// <summary>Inline Main-world health characteristic converter.</summary>
    [System.Serializable]
    public sealed class HealthSerializableConverter
        : CharacteristicSerializableConverter<HealthCharacteristic> { }
}
