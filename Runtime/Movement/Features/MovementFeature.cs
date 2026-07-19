using FFS.Libraries.StaticEcs;
 
 

namespace UniGame.StaticEcs.Features {
    using Unity;

    /// <summary>Main-world alias for <see cref="MovementFeature{TWorld}"/>.</summary>
    public sealed class MovementFeature : MovementFeature<Main> { }

    /// <summary>
    /// Registers <see cref="MovementDestinationComponent"/> with the world.
    /// Add <see cref="NavMeshMovementSystem{TWorld}"/> or <c>AstarMovementSystem&lt;TWorld&gt;</c>
    /// to the update group to drive actual navigation.
    /// </summary>
    public class MovementFeature<TWorld> : StaticEcsFeature<TWorld>
        where TWorld : struct, IWorldType {
        /// <inheritdoc/>
        public override void RegisterTypes(World<TWorld>.TypeRegistrar types) {
            types.Component<MovementDestinationComponent>();
        }
    }
}
