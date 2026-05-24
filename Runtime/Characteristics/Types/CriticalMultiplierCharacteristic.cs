namespace unigame.staticecs.features {
    /// <summary>
    /// Marker for the critical-multiplier characteristic. Used by the critical filter when the
    /// crit roll succeeds; defaults to 2.0 if the source entity has no value configured.
    /// </summary>
    [CharacteristicFlag(CharacteristicFlag.CriticalMultiplier)]
    public struct CriticalMultiplierCharacteristic : ICharacteristicType { }
}
