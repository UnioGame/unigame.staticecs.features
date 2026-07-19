namespace UniGame.StaticEcs.Features {
    using Unity;

    /// <summary>
    /// Main-default alias for <see cref="ModificationEffectFeature{TWorld, TStat}"/>.
    /// </summary>
    public sealed class ModificationEffectFeature<TStat> : ModificationEffectFeature<Main, TStat>
        where TStat : struct, ICharacteristicType {
        public ModificationEffectFeature(
            int maxStacks = 1,
            bool refreshOnReapply = true,
            short tickOrder = DefaultTickOrder,
            bool registerTickSystem = true)
            : base(maxStacks, refreshOnReapply, tickOrder, registerTickSystem) {
        }
    }
}
