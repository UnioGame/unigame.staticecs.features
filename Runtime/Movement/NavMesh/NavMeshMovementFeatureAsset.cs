namespace UniGame.StaticEcs.Features
{
    using UniGame.StaticEcs.Unity;
    using UnityEngine;

    /// <summary>Configures and creates the Main-world NavMesh movement feature.</summary>
    [CreateAssetMenu(
        menuName = "Static ECS/Features/NavMesh Movement",
        fileName = nameof(NavMeshMovementFeatureAsset))]
    public sealed class NavMeshMovementFeatureAsset :
        StaticEcsMainFeatureAsset<NavMeshMovementFeature>
    {
    }
}
