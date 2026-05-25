using unigame.staticecs.unity;

namespace unigame.staticecs.features {
    public sealed class TargetSelectionFeature : TargetSelectionFeature<Main> {
        public TargetSelectionFeature(
            bool registerRebuildSystem = true,
            short rebuildOrder = DefaultRebuildOrder)
            : base(registerRebuildSystem, rebuildOrder) {
        }
    }
}
