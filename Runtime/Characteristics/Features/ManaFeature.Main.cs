using unigame.staticecs.unity;

namespace unigame.staticecs.features {
    public sealed class ManaFeature : ManaFeature<Main> {
        public ManaFeature(bool registerRegen = true, short regenOrder = DefaultRegenOrder)
            : base(registerRegen, regenOrder) { }
    }
}
