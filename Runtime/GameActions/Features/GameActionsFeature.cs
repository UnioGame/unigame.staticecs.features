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
    public class GameActionsFeature<TWorld> : StaticEcsFeature<TWorld>
        where TWorld : struct, IWorldType {
        /// <inheritdoc/>
        public override void RegisterTypes(World<TWorld>.TypeRegistrar types) {
            types.Component<ActionMaskComponent>();
        }
    }
}
