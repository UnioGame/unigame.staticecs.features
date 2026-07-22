namespace UniGame.StaticEcs.Features
{
    /// <summary>
    /// Damage type tag carried by every damage event. Drives type-specific filter behavior
    /// (e.g. armor only mitigates <see cref="Physical"/>) and signals healing flow when set
    /// to <see cref="Healing"/>.
    /// </summary>
    public enum DamageType : byte
    {
        Physical = 0,
        Magical = 1,
        True = 2,
        Healing = 3,
    }
}
