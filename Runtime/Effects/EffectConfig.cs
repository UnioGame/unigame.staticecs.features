namespace UniGame.StaticEcs.Features
{
    using FFS.Libraries.StaticEcs;

    /// <summary>
    /// Per-effect stacking and re-apply policy. Set as a world resource by
    /// <c>EffectFeature&lt;TWorld, TEffect&gt;</c> and read by <c>EffectOperations.Apply</c> when
    /// the target already carries the same effect.
    /// </summary>
    public sealed class EffectConfig<TWorld, TEffect> : IResource
        where TWorld : struct, IWorldType
        where TEffect : struct, IEffectType
    {
        public int MaxStacks;
        public bool RefreshOnReapply;

        public EffectConfig(int maxStacks = 1, bool refreshOnReapply = true)
        {
            MaxStacks = maxStacks < 1 ? 1 : maxStacks;
            RefreshOnReapply = refreshOnReapply;
        }
    }
}
