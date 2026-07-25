namespace UniGame.StaticEcs.Features
{
    using Cysharp.Threading.Tasks;
    using FFS.Libraries.StaticEcs;
    using UniGame.Core.Runtime;

    /// <summary>
    /// Extends <see cref="MovementFeature{TWorld}"/> with <see cref="NavMeshAgentComponent"/>
    /// and optionally installs <see cref="NavMeshMovementSystem{TWorld}"/> according to
    /// <see cref="NavMeshMovementConfig.RegisterMovementSystem"/>.
    /// </summary>
    public class NavMeshMovementFeature<TWorld> :
        MovementFeature<TWorld>
        where TWorld : struct, IWorldType
    {
        public const short DefaultMovementOrder = 0;

        /// <summary>Whether the NavMesh driver system is installed.</summary>
        public bool registerMovementSystem = true;

        /// <summary>Execution order of the NavMesh driver.</summary>
        public short movementOrder = DefaultMovementOrder;

        /// <inheritdoc />
        public override UniTask InitializeAsync(ILifeTime lifeTime)
        {
            if (!World<TWorld>.HasResource<NavMeshMovementConfig>())
            {
                var configuration = new NavMeshMovementConfig
                {
                    RegisterMovementSystem = registerMovementSystem,
                    MovementOrder = movementOrder,
                };

                World<TWorld>.SetResource(configuration);
            }

            ref var config = ref World<TWorld>.GetResource<NavMeshMovementConfig>();
            var updateEnabled =
                World<TWorld>.HasResource<Unity.StaticEcsSystemsConfig>() &&
                World<TWorld>.GetResource<Unity.StaticEcsSystemsConfig>().update;
            if (updateEnabled && config.RegisterMovementSystem)
            {
                World<TWorld>.Systems<StaticEcsUpdateSystems>.Add(
                    new NavMeshMovementSystem<TWorld>(),
                    config.MovementOrder);
            }

            return UniTask.CompletedTask;
        }
    }

    /// <summary>Controls NavMesh movement system composition.</summary>
    public sealed class NavMeshMovementConfig : IResource
    {
        /// <summary>Whether the NavMesh driver system is installed.</summary>
        public bool RegisterMovementSystem = true;

        /// <summary>Execution order of the NavMesh driver.</summary>
        public short MovementOrder;
    }
}
