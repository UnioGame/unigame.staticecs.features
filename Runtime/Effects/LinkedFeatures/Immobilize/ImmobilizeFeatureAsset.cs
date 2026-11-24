namespace UniGame.StaticEcs.Features
{
    using UniGame.StaticEcs.Unity;
    using UnityEngine;

    /// <summary>Creates a fresh Main-world immobilize state feature.</summary>
    [CreateAssetMenu(menuName = "Static ECS/Features/Immobilize", fileName = nameof(ImmobilizeFeatureAsset))]
    public sealed class ImmobilizeFeatureAsset :
        StaticEcsMainFeatureAsset<ImmobilizeFeature>
    {
    }
}
