namespace UniGame.StaticEcs.Features
{
    using UniGame.StaticEcs.Unity;
    using UnityEngine;

    /// <summary>Creates a fresh Main-world stun state feature.</summary>
    [CreateAssetMenu(menuName = "Static ECS/Features/Stun", fileName = nameof(StunFeatureAsset))]
    public sealed class StunFeatureAsset : StaticEcsMainFeatureAsset<StunFeature> { }
}
