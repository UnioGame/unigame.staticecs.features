namespace UniGame.StaticEcs.Features
{
    using UniGame.StaticEcs.Unity;
    using UnityEngine;

    /// <summary>Creates a fresh Main-world standard characteristics feature.</summary>
    [CreateAssetMenu(
        menuName = "Static ECS/Features/Characteristics",
        fileName = nameof(CharacteristicsFeatureAsset)
    )]
    public sealed class CharacteristicsFeatureAsset :
        StaticEcsMainFeatureAsset<CharacteristicsFeature> { }
}
