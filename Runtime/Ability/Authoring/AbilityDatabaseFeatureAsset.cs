namespace UniGame.StaticEcs.Features
{
    using UniGame.StaticEcs.Unity;
    using UnityEngine;

    /// <summary>Publishes an authored ability database and creates its parameterless feature.</summary>
    [CreateAssetMenu(
        menuName = "Static ECS/Features/Ability Database",
        fileName = nameof(AbilityDatabaseFeatureAsset))]
    public sealed class AbilityDatabaseFeatureAsset :
        StaticEcsMainFeatureAsset<AbilityDatabaseFeature>
    {
    }
}
