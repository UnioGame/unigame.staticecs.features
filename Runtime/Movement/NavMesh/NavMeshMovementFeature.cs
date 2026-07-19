using FFS.Libraries.StaticEcs;
 
 

namespace UniGame.StaticEcs.Features {
    using Unity;

    /// <summary>Main-world alias for <see cref="NavMeshMovementFeature{TWorld}"/>.</summary>
    public sealed class NavMeshMovementFeature : NavMeshMovementFeature<Main> { }

    /// <summary>
    /// Extends <see cref="MovementFeature{TWorld}"/> with <see cref="NavMeshAgentComponent"/>
    /// for Unity NavMesh-driven navigation.
    /// Add <see cref="NavMeshMovementSystem{TWorld}"/> to the update group after registering.
    /// </summary>
    public class NavMeshMovementFeature<TWorld> : MovementFeature<TWorld>
        where TWorld : struct, IWorldType {
        /// <inheritdoc/>
        public override void RegisterTypes(World<TWorld>.TypeRegistrar types) {
            base.RegisterTypes(types);
            types.Component<NavMeshAgentComponent>();
        }
    }
}
