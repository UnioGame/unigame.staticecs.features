namespace UniGame.StaticEcs.Features
{
    using UniGame.StaticEcs.Unity;
    using UnityEngine;

    /// <summary>Creates a fresh Main-world damage pipeline feature.</summary>
    [CreateAssetMenu(
        menuName = "Static ECS/Features/Damage",
        fileName = nameof(DamageFeatureAsset)
    )]
    public sealed class DamageFeatureAsset : StaticEcsMainFeatureAsset<DamageFeature> { }
}
