namespace UniGame.StaticEcs.Features
{
    using FFS.Libraries.StaticEcs;

    /// <summary>
    /// Per-effect stacking and re-apply policy. Set as a world resource by
    /// <c>EffectFeature&lt;TWorld, TEffect&gt;</c> and read by <c>EffectOperations.Apply</c> when
    /// the target already carries the same effect.
    /// </summary>
    public class EffectConfig<TWorld, TEffect> : IResource
        where TWorld : struct, IWorldType
        where TEffect : struct, IEffectType
    {
        /// <summary>Maximum stack count for the effect.</summary>
        public int MaxStacks;

        /// <summary>Whether reapplication refreshes timing.</summary>
        public bool RefreshOnReapply;

        /// <summary>Execution order of the typed effect tick system.</summary>
        public short TickOrder;

        /// <summary>Whether the typed effect tick system is installed.</summary>
        public bool RegisterTickSystem;

        /// <summary>Creates an effect runtime configuration.</summary>
        public EffectConfig(
            int maxStacks = 1,
            bool refreshOnReapply = true,
            short tickOrder = 200,
            bool registerTickSystem = true)
        {
            MaxStacks = maxStacks < 1 ? 1 : maxStacks;
            RefreshOnReapply = refreshOnReapply;
            TickOrder = tickOrder;
            RegisterTickSystem = registerTickSystem;
        }
    }
}
