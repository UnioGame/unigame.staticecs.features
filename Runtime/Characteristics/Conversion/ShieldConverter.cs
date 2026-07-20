 
using UnityEngine;

namespace UniGame.StaticEcs.Features {
    using Unity;

    [AddComponentMenu("Static ECS/Characteristics/Shield Converter")]
    public sealed class ShieldConverter : CharacteristicConverter<Main, ShieldCharacteristic> { }

    /// <summary>Inline Main-world shield characteristic converter.</summary>
    [System.Serializable]
    public sealed class ShieldSerializableConverter : CharacteristicSerializableConverter<ShieldCharacteristic> { }
}
