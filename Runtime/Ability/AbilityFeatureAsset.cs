using UniGame.Core.Runtime;
using UniGame.StaticEcs.Unity;
using UnityEngine;

namespace UniGame.StaticEcs.Features
{
    /// <summary>Creates a fresh Main-world ability execution feature.</summary>
    [CreateAssetMenu(menuName = "Static ECS/Features/Ability", fileName = nameof(AbilityFeatureAsset))]
    public sealed class AbilityFeatureAsset : StaticEcsFeatureAsset
    {
        /// <summary>Whether the standard ability systems are installed.</summary>
        public bool registerSystems = true;
        /// <summary>Execution order of cast processing.</summary>
        public short castOrder = AbilityFeature.DefaultCastOrder;
        /// <summary>Execution order of wait processing.</summary>
        public short waitOrder = AbilityFeature.DefaultWaitOrder;
        /// <summary>Execution order of step progression.</summary>
        public short progressionOrder = AbilityFeature.DefaultProgressionOrder;

        /// <inheritdoc />
        public override IStaticEcsFeature<Main> CreateFeature(IContext context) =>
            new AbilityFeature(registerSystems, castOrder, waitOrder, progressionOrder);
    }
}
