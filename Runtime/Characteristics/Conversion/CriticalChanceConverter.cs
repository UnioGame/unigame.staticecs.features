using UnityEngine;

namespace UniGame.StaticEcs.Features {
    using Unity;

    [AddComponentMenu("Static ECS/Characteristics/Critical Chance Converter")]
    public sealed class CriticalChanceConverter : CharacteristicConverter<Main, CriticalChanceCharacteristic> { }

    /// <summary>Inline Main-world critical chance characteristic converter.</summary>
    [System.Serializable]
    public sealed class CriticalChanceSerializableConverter : CharacteristicSerializableConverter<CriticalChanceCharacteristic> { }
}
