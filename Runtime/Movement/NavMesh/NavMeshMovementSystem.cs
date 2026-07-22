namespace UniGame.StaticEcs.Features
{
    using FFS.Libraries.StaticEcs;
    using Unity;

    /// <summary>Main-world alias for <see cref="NavMeshMovementSystem{TWorld}"/>.</summary>
    public sealed class NavMeshMovementSystem : NavMeshMovementSystem<Main> { }

    /// <summary>
    /// Drives Unity <see cref="UnityEngine.AI.NavMeshAgent"/> from
    /// <see cref="MovementDestinationComponent"/> and <see cref="CharacteristicComponent{SpeedCharacteristic}"/>.
    /// Register in the update group after <see cref="NavMeshMovementFeature{TWorld}"/>.
    /// </summary>
    public class NavMeshMovementSystem<TWorld> : ISystem
        where TWorld : struct, IWorldType
    {
        /// <inheritdoc/>
        public void Update()
        {
            foreach (
                var entity in World<TWorld>
                    .Query<All<MovementDestinationComponent, NavMeshAgentComponent>>()
                    .Entities()
            )
            {
                ref readonly var dest = ref entity.Read<MovementDestinationComponent>();
                ref readonly var agent = ref entity.Read<NavMeshAgentComponent>();

                if (agent.Agent == null)
                {
                    continue;
                }

                if (entity.Has<CharacteristicComponent<SpeedCharacteristic>>())
                {
                    agent.Agent.speed = entity
                        .Read<CharacteristicComponent<SpeedCharacteristic>>()
                        .Value;
                }

                if (dest.IsActive)
                {
                    agent.Agent.SetDestination(dest.Destination);
                }
                else
                {
                    agent.Agent.ResetPath();
                }
            }
        }
    }
}
