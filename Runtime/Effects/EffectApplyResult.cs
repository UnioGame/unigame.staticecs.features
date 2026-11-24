namespace UniGame.StaticEcs.Features
{
    /// <summary>Outcome of <c>EffectOperations.Apply</c>.</summary>
    public enum EffectApplyResult
    {
        /// <summary>Nothing changed: the target is gone, or the parameters resolve to an empty effect.</summary>
        Failed = 0,

        /// <summary>A new effect started or was scheduled.</summary>
        Applied = 1,

        /// <summary>The target already carried the effect and it was refreshed or accumulated.</summary>
        Refreshed = 2,

        /// <summary>The target carries a conflicting effect with a higher priority.</summary>
        ConflictRejected = 3,

        /// <summary>The effect handler refused the application, see <c>IEffectHandler.CanApply</c>.</summary>
        Rejected = 4,
    }
}
