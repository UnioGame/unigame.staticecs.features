using unigame.staticecs.unity;

namespace unigame.staticecs.features {
    public sealed class AbilityDatabaseFeature : AbilityDatabaseFeature<Main> {
        public AbilityDatabaseFeature(AbilityDatabase database, bool instantiateAssets = true)
            : base(database, instantiateAssets) {
        }
    }
}
