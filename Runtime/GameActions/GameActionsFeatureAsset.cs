namespace UniGame.StaticEcs.Features
{
    using UniGame.StaticEcs.Unity;
    using UnityEngine;

    /// <summary>Creates a fresh Main-world game actions feature.</summary>
    [CreateAssetMenu(
        menuName = "Static ECS/Features/Game Actions",
        fileName = nameof(GameActionsFeatureAsset)
    )]
    public sealed class GameActionsFeatureAsset :
        StaticEcsMainFeatureAsset<GameActionsFeature>
    {
    }
}
