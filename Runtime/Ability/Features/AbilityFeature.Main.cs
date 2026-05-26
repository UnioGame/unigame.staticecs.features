using unigame.staticecs.unity;

namespace unigame.staticecs.features {
    public sealed class AbilityFeature : AbilityFeature<Main> {
        public AbilityFeature(
            bool registerSystems = true,
            short castOrder = DefaultCastOrder,
            short waitOrder = DefaultWaitOrder,
            short progressionOrder = DefaultProgressionOrder)
            : base(registerSystems, castOrder, waitOrder, progressionOrder) {
        }
    }
}
