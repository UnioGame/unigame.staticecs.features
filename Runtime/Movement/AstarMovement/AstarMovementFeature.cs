namespace UniGame.StaticEcs.Features
{
    using System.Threading;
    using Cysharp.Threading.Tasks;
    using FFS.Libraries.StaticEcs;

    /// <summary>
    /// Registers A* graph lifecycle, dynamic obstacle synchronization, and agent movement.
    /// </summary>
    public class AstarMovementFeature<TWorld>
        : MovementFeature<TWorld>,
            IStaticEcsSystemsFeature<TWorld, StaticEcsUpdateSystems>
        where TWorld : struct, IWorldType
    {
        /// <summary>Default order for graph initialization and obstacle synchronization.</summary>
        public const short DefaultGraphOrder = -100;

        /// <summary>Default order for agent movement synchronization.</summary>
        public const short DefaultMovementOrder = 0;

        /// <summary>Whether graph lifecycle synchronization is installed.</summary>
        public bool registerGraphSystem = true;

        /// <summary>Whether agent movement synchronization is installed.</summary>
        public bool registerMovementSystem = true;

        /// <summary>Execution order of graph synchronization.</summary>
        public short graphOrder = DefaultGraphOrder;

        /// <summary>Execution order of agent movement.</summary>
        public short movementOrder = DefaultMovementOrder;

        /// <summary>Creates the feature with optional graph and movement system registration.</summary>
        public AstarMovementFeature(
            bool registerGraphSystem = true,
            bool registerMovementSystem = true,
            short graphOrder = DefaultGraphOrder,
            short movementOrder = DefaultMovementOrder
        )
        {
            this.registerGraphSystem = registerGraphSystem;
            this.registerMovementSystem = registerMovementSystem;
            this.graphOrder = graphOrder;
            this.movementOrder = movementOrder;
        }

        /// <inheritdoc/>
        public override void RegisterTypes(World<TWorld>.TypeRegistrar types)
        {
            base.RegisterTypes(types);
            types
                .Component<AstarAIComponent>()
                .Component<AstarPathComponent>()
                .Component<AstarGridGraphConfigComponent>()
                .Component<AstarGridGraphComponent>()
                .Component<AstarObstacleComponent>()
                .Tag<AstarGraphInitializedTag>()
                .Tag<AstarGraphInitializationFailedTag>();
        }

        /// <summary>Registers graph synchronization before agent movement.</summary>
        public UniTask RegisterSystemsAsync(
            StaticEcsSystemsBuilder<TWorld, StaticEcsUpdateSystems> systems,
            CancellationToken cancellationToken
        )
        {
            if (registerGraphSystem)
            {
                systems.Add(new AstarGraphSystem<TWorld>(), graphOrder);
            }
            if (registerMovementSystem)
            {
                systems.Add(new AstarMovementSystem<TWorld>(), movementOrder);
            }
            return UniTask.CompletedTask;
        }
    }
}
