namespace UniGame.StaticEcs.Features
{
    using Unity;
    using UnityEngine;

    [AddComponentMenu("Static ECS/Characteristics/Mana Regen Converter")]
    public sealed class ManaRegenConverter : CharacteristicConverter<Main, ManaRegenCharacteristic> { }

    /// <summary>Inline Main-world mana regeneration characteristic converter.</summary>
    [System.Serializable]
    public sealed class ManaRegenSerializableConverter
        : CharacteristicSerializableConverter<ManaRegenCharacteristic> { }
}
