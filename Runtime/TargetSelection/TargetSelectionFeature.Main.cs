namespace UniGame.StaticEcs.Features
{
    using Unity;

    public sealed class TargetSelectionFeature : TargetSelectionFeature<Main>
    {
        public TargetSelectionFeature(
            bool registerRebuildSystem = true,
            short rebuildOrder = DefaultRebuildOrder
        )
            : base(registerRebuildSystem, rebuildOrder) { }
    }
}
