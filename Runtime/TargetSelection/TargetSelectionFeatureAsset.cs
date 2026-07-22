namespace UniGame.StaticEcs.Features
{
    using UniGame.Core.Runtime;
    using UniGame.StaticEcs.Unity;
    using UnityEngine;

    /// <summary>Creates a fresh Main-world target selection feature.</summary>
    [CreateAssetMenu(
        menuName = "Static ECS/Features/Target Selection",
        fileName = nameof(TargetSelectionFeatureAsset)
    )]
    public sealed class TargetSelectionFeatureAsset : StaticEcsFeatureAsset
    {
        /// <summary>Whether the target index rebuild system is installed.</summary>
        public bool registerRebuildSystem = true;

        /// <summary>Execution order of target index rebuilding.</summary>
        public short rebuildOrder = TargetSelectionFeature.DefaultRebuildOrder;

        /// <inheritdoc />
        public override IStaticEcsFeature<Main> CreateFeature(IContext context) =>
            new TargetSelectionFeature(registerRebuildSystem, rebuildOrder);
    }
}
