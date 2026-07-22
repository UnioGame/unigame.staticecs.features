namespace UniGame.StaticEcs.Features
{
    /// <summary>
    /// Marker for the armor-resist characteristic. Value is a 0..1 multiplicative reduction
    /// applied to physical-typed damage; non-physical types bypass the filter.
    /// </summary>
    [CharacteristicFlag(CharacteristicFlag.ArmorResist)]
    public struct ArmorResistCharacteristic : ICharacteristicType { }
}
