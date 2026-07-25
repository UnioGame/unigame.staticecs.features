namespace UniGame.StaticEcs.Features
{
    using UniGame.StaticEcs.Unity;
    using UnityEngine;

    /// <summary>Creates a fresh Main-world movement data feature.</summary>
    [CreateAssetMenu(
        menuName = "Static ECS/Features/Movement",
        fileName = nameof(MovementFeatureAsset)
    )]
    public sealed class MovementFeatureAsset :
        StaticEcsMainFeatureAsset<MovementFeature>
    {
    }
}
