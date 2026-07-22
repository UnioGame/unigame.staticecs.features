namespace UniGame.StaticEcs.Features
{
    using System.Threading;
    using Cysharp.Threading.Tasks;
    using FFS.Libraries.StaticEcs;

    /// <summary>
    /// Registers the <see cref="ActionMaskComponent"/> type that gates
    /// <see cref="GameActionOperations.Raise{TWorld,TAction}"/> calls.
    /// <para>
    /// Concrete action event types (<c>GameActionEvent&lt;TAction&gt;</c>) must be registered
    /// individually with <c>types.Event&lt;GameActionEvent&lt;TAction&gt;&gt;()</c> for each
    /// action kind that will be dispatched.
    /// </para>
    /// <para>
    /// To gate actions based on stun state, add
    /// <see cref="ActionMaskMaintenanceSystem{TWorld}"/> to the update group after
    /// <see cref="StunFeature{TWorld}"/> has been registered.
    /// </para>
    /// </summary>
    public class GameActionsFeature<TWorld>
        : StaticEcsFeature<TWorld>,
            IStaticEcsSystemsFeature<TWorld, StaticEcsUpdateSystems>
        where TWorld : struct, IWorldType
    {
        /// <summary>Whether action masks track stun events.</summary>
        public bool registerMaintenance = true;

        /// <summary>Execution order of action mask maintenance.</summary>
        public short maintenanceOrder = 25;

        /// <summary>Creates the action mask feature.</summary>
        public GameActionsFeature(bool registerMaintenance = true, short maintenanceOrder = 25)
        {
            this.registerMaintenance = registerMaintenance;
            this.maintenanceOrder = maintenanceOrder;
        }

        /// <inheritdoc/>
        public override void RegisterTypes(World<TWorld>.TypeRegistrar types)
        {
            types.Component<ActionMaskComponent>();
        }

        /// <inheritdoc />
        public UniTask RegisterSystemsAsync(
            StaticEcsSystemsBuilder<TWorld, StaticEcsUpdateSystems> systems,
            CancellationToken cancellationToken
        )
        {
            if (registerMaintenance)
            {
                systems.Add(new ActionMaskMaintenanceSystem<TWorld>(), maintenanceOrder);
            }

            return UniTask.CompletedTask;
        }
    }
}
