 
using UnityEngine;

namespace UniGame.StaticEcs.Features {
    using Unity;

    [AddComponentMenu("Static ECS/Characteristics/Armor Resist Converter")]
    public sealed class ArmorResistConverter : CharacteristicConverter<Main, ArmorResistCharacteristic> { }

    /// <summary>Inline Main-world armor resistance characteristic converter.</summary>
    [System.Serializable]
    public sealed class ArmorResistSerializableConverter : CharacteristicSerializableConverter<ArmorResistCharacteristic> { }
}
