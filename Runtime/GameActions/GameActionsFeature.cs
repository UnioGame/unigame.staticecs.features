using System.Threading;
using Cysharp.Threading.Tasks;
using FFS.Libraries.StaticEcs;

namespace UniGame.StaticEcs.Features {
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
    public class GameActionsFeature<TWorld> :
        StaticEcsFeature<TWorld>,
        IStaticEcsSystemsFeature<TWorld, StaticEcsUpdateSystems>
        where TWorld : struct, IWorldType {
        private readonly bool _registerMaintenance;
        private readonly short _maintenanceOrder;

        /// <summary>Creates the action mask feature.</summary>
        public GameActionsFeature(bool registerMaintenance = true, short maintenanceOrder = 25) {
            _registerMaintenance = registerMaintenance;
            _maintenanceOrder = maintenanceOrder;
        }

        /// <inheritdoc/>
        public override void RegisterTypes(World<TWorld>.TypeRegistrar types) {
            types.Component<ActionMaskComponent>();
        }

        /// <inheritdoc />
        public UniTask RegisterSystemsAsync(
            StaticEcsSystemsBuilder<TWorld, StaticEcsUpdateSystems> systems,
            CancellationToken cancellationToken) {
            if (_registerMaintenance) {
                systems.Add(new ActionMaskMaintenanceSystem<TWorld>(), _maintenanceOrder);
            }

            return UniTask.CompletedTask;
        }
    }
}
