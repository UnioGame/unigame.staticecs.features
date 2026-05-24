namespace unigame.staticecs.features {
    /// <summary>
    /// Marker for the dodge-chance characteristic. Value is a 0..1 probability to fully avoid an
    /// incoming damage event before any other filter in the damage pipeline runs.
    /// </summary>
    [CharacteristicFlag(CharacteristicFlag.DodgeChance)]
    public struct DodgeChanceCharacteristic : ICharacteristicType { }
}
