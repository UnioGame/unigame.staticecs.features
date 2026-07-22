namespace UniGame.StaticEcs.Features
{
    using FFS.Libraries.StaticEcs;

    /// <summary>
    /// Raised when the dodge filter cancels an incoming damage event.
    /// </summary>
    public struct DamageDodgedEvent : IEvent
    {
        public EntityGID Source;
        public EntityGID Target;
        public float Amount;
        public DamageType Type;
    }
}
