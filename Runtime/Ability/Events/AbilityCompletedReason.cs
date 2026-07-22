namespace UniGame.StaticEcs.Features
{
    /// <summary>
    /// Why a cast-entity terminated. Read by business-layer subscribers of
    /// <see cref="AbilityCompletedEvent"/> to e.g. issue resource refunds on Cancelled / Interrupted.
    /// </summary>
    public enum AbilityCompletedReason : byte
    {
        Success = 0,
        Cancelled = 1,
        Interrupted = 2,
    }
}
