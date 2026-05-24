namespace unigame.staticecs.features {
    /// <summary>
    /// Marker for the critical-chance characteristic. Value is a 0..1 probability for an incoming
    /// damage event to be amplified by <see cref="CriticalMultiplierCharacteristic"/>.
    /// </summary>
    [CharacteristicFlag(CharacteristicFlag.CriticalChance)]
    public struct CriticalChanceCharacteristic : ICharacteristicType { }
}
