namespace UniGame.StaticEcs.Features
{
    using FFS.Libraries.StaticEcs;

    /// <summary>
    /// Per-effect-type behaviour hook. Registered as an <c>IEffectHandler&lt;TWorld, TEffect&gt;</c>
    /// world resource by <c>EffectFeature</c>; <c>EffectOperations</c> and <c>EffectTickSystem</c>
    /// dispatch lifecycle calls to the registered instance.
    ///
    /// Implementations must be stateless or safe to share — only one handler per (world, effect)
    /// pair is supported. An <see cref="EffectDurationMode.Instant"/> effect goes through the same calls as any other:
    /// <see cref="OnApplied"/> and then, at once, <see cref="OnRemoved"/> with <c>expired</c> true; it never ticks.
    /// </summary>
    public interface IEffectHandler<TWorld, TEffect> : IResource
        where TWorld : struct, IWorldType
        where TEffect : struct, IEffectType
    {
        /// <summary>
        /// Called before anything changes on the target, for fresh applications and re-applies.
        /// Returning false rejects the application with <see cref="EffectApplyResult.Rejected"/>.
        /// </summary>
        bool CanApply(in EffectApplyContext context);

        /// <summary>Called from <c>Apply</c> for both fresh applications and refresh re-applies.</summary>
        void OnApplied(in EffectApplyContext context);

        /// <summary>Called once per <see cref="EffectComponent{TEffect}.Period"/> tick.</summary>
        void OnTick(EntityGID target, EntityGID source, int stacks);

        /// <summary>
        /// Called once before the effect leaves the target. <paramref name="expired"/> is true on
        /// natural lifetime end, false on manual removal or source-dead cleanup.
        /// </summary>
        void OnRemoved(EntityGID target, EntityGID source, int stacks, bool expired);
    }
}
