 

namespace UniGame.StaticEcs.Features {
    using Unity;

    /// <summary>
    /// Main-default alias for <see cref="HealOverTimeFeature{TWorld}"/>.
    /// </summary>
    public sealed class HealOverTimeFeature : HealOverTimeFeature<Main> {
        public HealOverTimeFeature(
            int maxStacks = 5,
            bool refreshOnReapply = true,
            short tickOrder = DefaultTickOrder,
            bool registerTickSystem = true)
            : base(maxStacks, refreshOnReapply, tickOrder, registerTickSystem) {
        }
    }
}
