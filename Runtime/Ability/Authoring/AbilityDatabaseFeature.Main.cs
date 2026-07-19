namespace UniGame.StaticEcs.Features {
    using Unity;

    public sealed class AbilityDatabaseFeature : AbilityDatabaseFeature<Main> {
        public AbilityDatabaseFeature(AbilityDatabase database, bool instantiateAssets = true)
            : base(database, instantiateAssets) {
        }
    }
}
