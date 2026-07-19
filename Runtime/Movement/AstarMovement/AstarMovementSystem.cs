using FFS.Libraries.StaticEcs;
using Pathfinding;

namespace UniGame.StaticEcs.Features {
    /// <summary>
    /// Drives A* Pathfinding Project <see cref="Pathfinding.IAstarAI"/> from
    /// <see cref="MovementDestinationComponent"/> and <see cref="CharacteristicComponent{SpeedCharacteristic}"/>.
    /// Register in the update group after <see cref="AstarMovementFeature{TWorld}"/>.
    /// </summary>
    public class AstarMovementSystem<TWorld> : ISystem
        where TWorld : struct, IWorldType {
        private const float DestinationChangeSqrThreshold = 0.0001f;

        /// <inheritdoc/>
        public void Update() {
            foreach (var entity in World<TWorld>
                         .Query<All<MovementDestinationComponent, AstarAIComponent>>()
                         .Entities()) {
                ref readonly var dest  = ref entity.Read<MovementDestinationComponent>();
                ref var          astar = ref entity.Mut<AstarAIComponent>();

                if (astar.AI == null) {
                    continue;
                }

                // Explicit ECS requests are the only path request source, including while the graph is initializing.
                astar.AI.canSearch = false;

                if (entity.Has<CharacteristicComponent<SpeedCharacteristic>>()) {
                    astar.AI.maxSpeed = entity.Read<CharacteristicComponent<SpeedCharacteristic>>().Value;
                }

                if (dest.IsActive) {
                    astar.AI.destination = dest.Destination;

                    if (!TryPrepareGraph(ref astar)) {
                        astar.AI.isStopped = true;
                        continue;
                    }

                    astar.AI.isStopped = false;

                    var destinationChanged = !astar.HasRequestedDestination
                                             || (astar.LastRequestedDestination - dest.Destination).sqrMagnitude
                                             > DestinationChangeSqrThreshold;

                    if (destinationChanged && !astar.AI.pathPending) {
                        astar.AI.SearchPath();
                        astar.LastRequestedDestination = dest.Destination;
                        astar.HasRequestedDestination = true;
                    }
                }
                else {
                    astar.AI.isStopped = true;
                    astar.HasRequestedDestination = false;
                }
            }
        }

        private static bool TryPrepareGraph(ref AstarAIComponent astar) {
            if (astar.Seeker == null
                || !astar.GraphEntity.TryUnpack<TWorld>(out var graphEntity)
                || !graphEntity.Has<AstarGraphInitializedTag>()
                || !graphEntity.Has<AstarPathComponent>()
                || !graphEntity.Has<AstarGridGraphRuntimeComponent>()) {
                return false;
            }

            var backend = graphEntity.Read<AstarPathComponent>().Backend;
            var graph = graphEntity.Read<AstarGridGraphRuntimeComponent>().Graph;
            if (backend == null || backend != AstarPath.active || backend.data == null
                || graph == null || graph.active != backend || !graph.isScanned
                || System.Array.IndexOf(backend.data.graphs, graph) < 0) {
                return false;
            }

            var graphMask = GraphMask.FromGraph(graph);
            astar.Seeker.graphMask = graphMask;

            var constraint = NearestNodeConstraint.Walkable;
            constraint.graphMask = graphMask;
            var nearest = backend.GetNearest(astar.AI.position, constraint).node;
            return nearest != null && nearest.Walkable;
        }
    }
}
