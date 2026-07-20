 
using UnityEngine;

namespace UniGame.StaticEcs.Features {
    using Unity;

    [AddComponentMenu("Static ECS/Characteristics/Mana Converter")]
    public sealed class ManaConverter : CharacteristicConverter<Main, ManaCharacteristic> { }

    /// <summary>Inline Main-world mana characteristic converter.</summary>
    [System.Serializable]
    public sealed class ManaSerializableConverter : CharacteristicSerializableConverter<ManaCharacteristic> { }
}
