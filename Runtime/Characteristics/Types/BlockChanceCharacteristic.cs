namespace UniGame.StaticEcs.Features {
    /// <summary>
    /// Marker for the block-chance characteristic. Value is interpreted as a 0..1 probability
    /// to fully block an incoming damage event when the target carries <see cref="BlockableTag"/>.
    /// </summary>
    [CharacteristicFlag(CharacteristicFlag.BlockChance)]
    public struct BlockChanceCharacteristic : ICharacteristicType { }
}
