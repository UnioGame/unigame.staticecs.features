namespace UniGame.StaticEcs.Features
{
    using UniGame.StaticEcs.Unity;
    using UnityEngine;

    /// <summary>Creates a fresh Main-world standard effects feature.</summary>
    [CreateAssetMenu(
        menuName = "Static ECS/Features/Effects",
        fileName = nameof(EffectsFeatureAsset)
    )]
    public sealed class EffectsFeatureAsset : StaticEcsMainFeatureAsset<EffectsFeature> { }
}
