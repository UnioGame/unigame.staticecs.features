namespace UniGame.StaticEcs.Features
{
    using Cysharp.Threading.Tasks;
    using FFS.Libraries.StaticEcs;
    using UniGame.Core.Runtime;

    /// <summary>
    /// Registers A* graph lifecycle, dynamic obstacle synchronization, and agent movement.
    /// </summary>
    public class AstarMovementFeature<TWorld>
        : MovementFeature<TWorld>
        where TWorld : struct, IWorldType
    {
        /// <summary>Default order for graph initialization and obstacle synchronization.</summary>
        public const short DefaultGraphOrder = -100;

        /// <summary>Default order for agent movement synchronization.</summary>
        public const short DefaultMovementOrder = 0;

        /// <summary>Whether graph synchronization is installed.</summary>
        public bool registerGraphSystem = true;

        /// <summary>Whether A* agent synchronization is installed.</summary>
        public bool registerMovementSystem = true;

        /// <summary>Execution order of graph synchronization.</summary>
        public short graphOrder = DefaultGraphOrder;

        /// <summary>Execution order of agent synchronization.</summary>
        public short movementOrder = DefaultMovementOrder;

        /// <inheritdoc />
        public override UniTask InitializeAsync(ILifeTime lifeTime)
        {
            if (!World<TWorld>.HasResource<AstarMovementConfig>())
            {
                var configuration = new AstarMovementConfig
                {
                    RegisterGraphSystem = registerGraphSystem,
                    RegisterMovementSystem = registerMovementSystem,
                    GraphOrder = graphOrder,
                    MovementOrder = movementOrder,
                };

                World<TWorld>.SetResource(configuration);
            }

            ref var config = ref World<TWorld>.GetResource<AstarMovementConfig>();
            if (config.RegisterGraphSystem)
                World<TWorld>.Systems<StaticEcsUpdateSystems>.Add(
                    new AstarGraphSystem<TWorld>(),
                    config.GraphOrder);
            if (config.RegisterMovementSystem)
                World<TWorld>.Systems<StaticEcsUpdateSystems>.Add(
                    new AstarMovementSystem<TWorld>(),
                    config.MovementOrder);
            return UniTask.CompletedTask;
        }
    }

    /// <summary>Controls A* movement systems and execution order.</summary>
    public sealed class AstarMovementConfig : IResource
    {
        /// <summary>Whether graph synchronization is installed.</summary>
        public bool RegisterGraphSystem = true;

        /// <summary>Whether A* agent synchronization is installed.</summary>
        public bool RegisterMovementSystem = true;

        /// <summary>Execution order of graph synchronization.</summary>
        public short GraphOrder = -100;

        /// <summary>Execution order of agent synchronization.</summary>
        public short MovementOrder;
    }
}
