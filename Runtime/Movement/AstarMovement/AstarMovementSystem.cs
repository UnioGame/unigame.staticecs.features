namespace UniGame.StaticEcs.Features
{
    using FFS.Libraries.StaticEcs;
    using Pathfinding;

    /// <summary>
    /// Drives A* Pathfinding Project <see cref="Pathfinding.IAstarAI"/> from
    /// <see cref="MovementDestinationComponent"/> and <see cref="CharacteristicComponent{SpeedCharacteristic}"/>.
    /// Register in the update group after <see cref="AstarMovementFeature{TWorld}"/>.
    /// </summary>
    public class AstarMovementSystem<TWorld> : ISystem
        where TWorld : struct, IWorldType
    {
        private const float DestinationChangeSqrThreshold = 0.0001f;

        /// <inheritdoc/>
        public void Update()
        {
            foreach (
                var entity in World<TWorld>
                    .Query<All<MovementDestinationComponent, AstarAIComponent>>()
                    .Entities()
            )
            {
                ref readonly var dest = ref entity.Read<MovementDestinationComponent>();
                ref var astar = ref entity.Mut<AstarAIComponent>();

                if (astar.AI == null)
                {
                    continue;
                }

                // Explicit ECS requests are the only path request source, including while the graph is initializing.
                astar.AI.canSearch = false;

                if (entity.Has<CharacteristicComponent<SpeedCharacteristic>>())
                {
                    astar.AI.maxSpeed = entity
                        .Read<CharacteristicComponent<SpeedCharacteristic>>()
                        .Value;
                }

                if (dest.IsActive)
                {
                    astar.AI.destination = dest.Destination;

                    if (!TryPrepareGraph(ref astar))
                    {
                        astar.AI.isStopped = true;
                        continue;
                    }

                    astar.AI.isStopped = false;

                    var destinationChanged =
                        !astar.HasRequestedDestination
                        || (astar.LastRequestedDestination - dest.Destination).sqrMagnitude
                            > DestinationChangeSqrThreshold;

                    if (destinationChanged && !astar.AI.pathPending)
                    {
                        astar.AI.SearchPath();
                        astar.LastRequestedDestination = dest.Destination;
                        astar.HasRequestedDestination = true;
                    }
                }
                else
                {
                    astar.AI.isStopped = true;
                    astar.HasRequestedDestination = false;
                }
            }
        }

        private static bool TryPrepareGraph(ref AstarAIComponent astar)
        {
            if (
                astar.Seeker == null
                || !astar.GraphEntity.TryUnpack<TWorld>(out var graphEntity)
                || !graphEntity.Has<AstarGraphInitializedTag>()
                || !graphEntity.Has<AstarPathComponent>()
                || !graphEntity.Has<AstarGridGraphComponent>()
            )
            {
                return false;
            }

            var backend = graphEntity.Read<AstarPathComponent>().Backend;
            var graph = graphEntity.Read<AstarGridGraphComponent>().Graph;
            if (
                backend == null
                || backend != AstarPath.active
                || backend.data == null
                || graph == null
                || graph.active != backend
                || !graph.isScanned
                || System.Array.IndexOf(backend.data.graphs, graph) < 0
            )
            {
                return false;
            }

            var graphMask = GraphMask.FromGraph(graph);
            astar.Seeker.graphMask = graphMask;

            // IAstarAI.position can depend on AIBase's private transform cache, which is not
            // initialized for disabled authoring/test components. Seeker is already required
            // and provides the same agent transform without that lifecycle coupling.
            var agentPosition = astar.Seeker.transform.position;
            // Use the backend query so its max-nearest-node distance remains authoritative.
            // The graph index check keeps this feature isolated to its configured graph.
            var nearest = backend.GetNearest(agentPosition, NearestNodeConstraint.None).node;
            return nearest != null && nearest.Walkable && nearest.GraphIndex == graph.graphIndex;
        }
    }
}
