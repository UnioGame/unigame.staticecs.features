 
using UnityEngine;

namespace UniGame.StaticEcs.Features {
    using Unity;

    [AddComponentMenu("Static ECS/Characteristics/Dodge Chance Converter")]
    public sealed class DodgeChanceConverter : CharacteristicConverter<Main, DodgeChanceCharacteristic> { }

    /// <summary>Inline Main-world dodge chance characteristic converter.</summary>
    [System.Serializable]
    public sealed class DodgeChanceSerializableConverter : CharacteristicSerializableConverter<DodgeChanceCharacteristic> { }
}
