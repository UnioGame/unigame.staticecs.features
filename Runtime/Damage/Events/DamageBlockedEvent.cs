namespace UniGame.StaticEcs.Features
{
    using FFS.Libraries.StaticEcs;

    /// <summary>
    /// Raised when the block filter cancels an incoming damage event after a successful roll
    /// against the target's <see cref="BlockChanceCharacteristic"/>.
    /// </summary>
    public struct DamageBlockedEvent : IEvent
    {
        public EntityGID Source;
        public EntityGID Target;
        public float BlockedAmount;
        public DamageType Type;
    }
}
