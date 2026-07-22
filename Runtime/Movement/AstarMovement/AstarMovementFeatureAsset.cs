namespace UniGame.StaticEcs.Features
{
    using UniGame.Core.Runtime;
    using UniGame.StaticEcs.Unity;
    using UnityEngine;

    /// <summary>Creates a fresh Main-world A* movement feature.</summary>
    [CreateAssetMenu(
        menuName = "Static ECS/Features/Astar Movement",
        fileName = nameof(AstarMovementFeatureAsset)
    )]
    public sealed class AstarMovementFeatureAsset : StaticEcsFeatureAsset
    {
        /// <summary>Whether graph lifecycle synchronization is installed.</summary>
        public bool registerGraphSystem = true;

        /// <summary>Whether agent movement synchronization is installed.</summary>
        public bool registerMovementSystem = true;

        /// <summary>Execution order of graph synchronization.</summary>
        public short graphOrder = AstarMovementFeature.DefaultGraphOrder;

        /// <summary>Execution order of agent movement.</summary>
        public short movementOrder = AstarMovementFeature.DefaultMovementOrder;

        /// <inheritdoc />
        public override IStaticEcsFeature<Main> CreateFeature(IContext context) =>
            new AstarMovementFeature(
                registerGraphSystem,
                registerMovementSystem,
                graphOrder,
                movementOrder
            );
    }
}
