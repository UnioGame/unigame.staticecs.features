using FFS.Libraries.StaticEcs;
using unigame.staticecs;

namespace unigame.staticecs.features {
    /// <summary>
    /// Registers A* graph lifecycle, dynamic obstacle synchronization, and agent movement.
    /// </summary>
    public class AstarMovementFeature<TWorld> : MovementFeature<TWorld>,
        IStaticEcsSystemsFeature<TWorld, StaticEcsUpdateSystems>
        where TWorld : struct, IWorldType {
        /// <summary>Default order for graph initialization and obstacle synchronization.</summary>
        public const short DefaultGraphOrder = -100;
        /// <summary>Default order for agent movement synchronization.</summary>
        public const short DefaultMovementOrder = 0;

        private readonly bool _registerGraphSystem;
        private readonly bool _registerMovementSystem;
        private readonly short _graphOrder;
        private readonly short _movementOrder;

        /// <summary>Creates the feature with optional graph and movement system registration.</summary>
        public AstarMovementFeature(
            bool registerGraphSystem = true,
            bool registerMovementSystem = true,
            short graphOrder = DefaultGraphOrder,
            short movementOrder = DefaultMovementOrder) {
            _registerGraphSystem = registerGraphSystem;
            _registerMovementSystem = registerMovementSystem;
            _graphOrder = graphOrder;
            _movementOrder = movementOrder;
        }

        /// <inheritdoc/>
        public override void RegisterTypes(World<TWorld>.TypeRegistrar types) {
            base.RegisterTypes(types);
            types
                .Component<AstarAIComponent>()
                .Component<AstarPathComponent>()
                .Component<AstarGridGraphConfigComponent>()
                .Component<AstarGridGraphRuntimeComponent>()
                .Component<AstarObstacleComponent>()
                .Tag<AstarGraphInitializedTag>()
                .Tag<AstarGraphInitializationFailedTag>();
        }

        /// <summary>Registers graph synchronization before agent movement.</summary>
        public void RegisterSystems(StaticEcsSystemsBuilder<TWorld, StaticEcsUpdateSystems> systems) {
            if (_registerGraphSystem) {
                systems.Add(new AstarGraphSystem<TWorld>(), _graphOrder);
            }
            if (_registerMovementSystem) {
                systems.Add(new AstarMovementSystem<TWorld>(), _movementOrder);
            }
        }
    }
}
