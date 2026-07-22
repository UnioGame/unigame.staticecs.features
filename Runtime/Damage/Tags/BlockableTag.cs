namespace UniGame.StaticEcs.Features
{
    using FFS.Libraries.StaticEcs;

    /// <summary>
    /// Marks an entity as eligible for the block filter. Without this tag the block roll is
    /// skipped regardless of <see cref="BlockChanceCharacteristic"/> value.
    /// </summary>
    public struct BlockableTag : ITag
    {
    }
}
