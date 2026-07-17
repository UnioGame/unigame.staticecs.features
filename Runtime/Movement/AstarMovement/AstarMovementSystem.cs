using FFS.Libraries.StaticEcs;
using unigame.staticecs.unity;

namespace unigame.staticecs.features {
    /// <summary>Main-world alias for <see cref="AstarMovementSystem{TWorld}"/>.</summary>
    public sealed class AstarMovementSystem : AstarMovementSystem<Main> { }

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

                if (entity.Has<CharacteristicComponent<SpeedCharacteristic>>()) {
                    astar.AI.maxSpeed = entity.Read<CharacteristicComponent<SpeedCharacteristic>>().Value;
                }

                if (dest.IsActive) {
                    astar.AI.destination = dest.Destination;
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
    }
}
