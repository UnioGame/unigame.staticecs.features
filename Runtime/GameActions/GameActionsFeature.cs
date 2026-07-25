namespace UniGame.StaticEcs.Features
{
    using Cysharp.Threading.Tasks;
    using FFS.Libraries.StaticEcs;
    using UniGame.Core.Runtime;

    /// <summary>
    /// Registers the <see cref="ActionMaskComponent"/> type that gates
    /// <see cref="GameActionOperations.Raise{TWorld,TAction}"/> calls.
    /// <para>
    /// Concrete action event types and stable ids must be registered individually with
    /// <see cref="GameActionRegistrar"/> for each action kind that will be dispatched.
    /// </para>
    /// <para>
    /// To gate actions based on stun state, add
    /// <see cref="ActionMaskMaintenanceSystem{TWorld}"/> to the update group after
    /// <see cref="StunFeature{TWorld}"/> has been registered.
    /// </para>
    /// </summary>
    public class GameActionsFeature<TWorld> : StaticEcsFeature<TWorld>
        where TWorld : struct, IWorldType
    {
        /// <summary>Whether the action-mask maintenance system is installed.</summary>
        public bool registerMaintenance = true;

        /// <summary>Execution order of action-mask maintenance.</summary>
        public short maintenanceOrder = 25;

        /// <inheritdoc />
        public override UniTask InitializeAsync(ILifeTime lifeTime)
        {
            if (!World<TWorld>.HasResource<GameActionRegistry<TWorld>>())
            {
                var registry = new GameActionRegistry<TWorld>();
                World<TWorld>.SetResource(registry);
            }

            if (!World<TWorld>.HasResource<GameActionsConfig>())
            {
                var configuration = new GameActionsConfig
                {
                    RegisterMaintenance = registerMaintenance,
                    MaintenanceOrder = maintenanceOrder,
                };

                World<TWorld>.SetResource(configuration);
            }

            ref var config = ref World<TWorld>.GetResource<GameActionsConfig>();
            var updateEnabled =
                World<TWorld>.HasResource<Unity.StaticEcsSystemsConfig>() &&
                World<TWorld>.GetResource<Unity.StaticEcsSystemsConfig>().update;
            if (updateEnabled && config.RegisterMaintenance)
            {
                World<TWorld>.Systems<StaticEcsUpdateSystems>.Add(
                    new ActionMaskMaintenanceSystem<TWorld>(),
                    config.MaintenanceOrder);
            }

            return UniTask.CompletedTask;
        }
    }
}
