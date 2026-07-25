namespace UniGame.StaticEcs.Features
{
    using UniGame.StaticEcs.Unity;
    using UnityEngine;

    /// <summary>Creates a fresh Main-world target selection feature.</summary>
    [CreateAssetMenu(
        menuName = "Static ECS/Features/Target Selection",
        fileName = nameof(TargetSelectionFeatureAsset)
    )]
    public sealed class TargetSelectionFeatureAsset :
        StaticEcsMainFeatureAsset<TargetSelectionFeature>
    {
    }
}
