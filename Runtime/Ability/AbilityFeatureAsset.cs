namespace UniGame.StaticEcs.Features
{
    using UniGame.StaticEcs.Unity;
    using UnityEngine;

    /// <summary>Creates a fresh Main-world ability execution feature.</summary>
    [CreateAssetMenu(
        menuName = "Static ECS/Features/Ability",
        fileName = nameof(AbilityFeatureAsset)
    )]
    public sealed class AbilityFeatureAsset :
        StaticEcsMainFeatureAsset<AbilityFeature>
    {
    }
}
