 
using UnityEngine;

namespace UniGame.StaticEcs.Features {
    using Unity;

    [AddComponentMenu("Static ECS/Characteristics/Critical Multiplier Converter")]
    public sealed class CriticalMultiplierConverter : CharacteristicConverter<Main, CriticalMultiplierCharacteristic> { }

    /// <summary>Inline Main-world critical multiplier characteristic converter.</summary>
    [System.Serializable]
    public sealed class CriticalMultiplierSerializableConverter : CharacteristicSerializableConverter<CriticalMultiplierCharacteristic> { }
}
