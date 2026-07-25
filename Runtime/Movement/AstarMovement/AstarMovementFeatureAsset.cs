namespace UniGame.StaticEcs.Features
{
    using UniGame.StaticEcs.Unity;
    using UnityEngine;

    /// <summary>Creates a fresh Main-world A* movement feature.</summary>
    [CreateAssetMenu(
        menuName = "Static ECS/Features/Astar Movement",
        fileName = nameof(AstarMovementFeatureAsset)
    )]
    public sealed class AstarMovementFeatureAsset :
        StaticEcsMainFeatureAsset<AstarMovementFeature>
    {
    }
}
