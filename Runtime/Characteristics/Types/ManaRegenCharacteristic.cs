namespace UniGame.StaticEcs.Features
{
    /// <summary>Mana restored per second; the consumer applies it with its own system.</summary>
    [CharacteristicFlag(CharacteristicFlag.ManaRegen)]
    public struct ManaRegenCharacteristic : ICharacteristicType { }
}
