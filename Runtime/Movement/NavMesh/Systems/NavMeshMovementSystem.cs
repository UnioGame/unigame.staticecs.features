namespace UniGame.StaticEcs.Features
{
    using FFS.Libraries.StaticEcs;
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
                    var speed = entity
                        .Read<CharacteristicComponent<SpeedCharacteristic>>()
                        .Value;
                    if (!UnityEngine.Mathf.Approximately(agent.Agent.speed, speed))
                    {
                        agent.Agent.speed = speed;
                    }
                }

                if (dest.IsActive)
                {
                    if (!agent.Agent.hasPath ||
                        (agent.Agent.destination - dest.Destination).sqrMagnitude > 0.0001f)
                    {
                        agent.Agent.SetDestination(dest.Destination);
                    }
                }
                else if (agent.Agent.hasPath)
                {
                    agent.Agent.ResetPath();
                }
            }
        }
    }
}
