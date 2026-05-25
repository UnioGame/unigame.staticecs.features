using unigame.staticecs.unity;

namespace unigame.staticecs.features {
    /// <summary>
    /// Main-default alias for <see cref="StunEffectFeature{TWorld}"/>.
    /// </summary>
    public sealed class StunEffectFeature : StunEffectFeature<Main> {
        public StunEffectFeature(
            short tickOrder = DefaultTickOrder,
            bool registerTickSystem = true)
            : base(tickOrder, registerTickSystem) {
        }
    }
}
