namespace UniGame.StaticEcs.Features
{
    using UniGame.StaticEcs.Unity;

    /// <summary>Creates exact-type Main-world configuration resources for an effect type.</summary>
    public static class EffectConfig<TEffect>
        where TEffect : struct, IEffectType
    {
        /// <summary>Creates a default-world effect runtime configuration.</summary>
        public static EffectConfig<Main, TEffect> Create(
            int maxStacks = 1,
            bool refreshOnReapply = true,
            short tickOrder = 200,
            bool registerTickSystem = true)
        {
            return new EffectConfig<Main, TEffect>(
                maxStacks,
                refreshOnReapply,
                tickOrder,
                registerTickSystem);
        }
    }
}
