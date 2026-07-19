using UniGame.Core.Runtime;
using UniGame.StaticEcs.Unity;
using UnityEngine;

namespace UniGame.StaticEcs.Features
{
    /// <summary>Creates a fresh Main-world game actions feature.</summary>
    [CreateAssetMenu(menuName = "Static ECS/Features/Game Actions", fileName = nameof(GameActionsFeatureAsset))]
    public sealed class GameActionsFeatureAsset : StaticEcsFeatureAsset
    {
        /// <summary>Whether action masks track stun events.</summary>
        public bool registerMaintenance = true;
        /// <summary>Execution order of action mask maintenance.</summary>
        public short maintenanceOrder = 25;

        /// <inheritdoc />
        public override IStaticEcsFeature<Main> CreateFeature(IContext context) =>
            new GameActionsFeature(registerMaintenance, maintenanceOrder);
    }
}
