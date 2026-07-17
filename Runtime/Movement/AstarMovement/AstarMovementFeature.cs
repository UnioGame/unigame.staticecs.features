using FFS.Libraries.StaticEcs;
using unigame.staticecs;
using unigame.staticecs.unity;

namespace unigame.staticecs.features {
    /// <summary>Main-world alias for <see cref="AstarMovementFeature{TWorld}"/>.</summary>
    public sealed class AstarMovementFeature : AstarMovementFeature<Main> { }

    /// <summary>
    /// Extends <see cref="MovementFeature{TWorld}"/> with <see cref="AstarAIComponent"/>
    /// for A* Pathfinding Project-driven navigation.
    /// Add <see cref="AstarMovementSystem{TWorld}"/> to the update group after registering.
    /// </summary>
    public class AstarMovementFeature<TWorld> : MovementFeature<TWorld>
        where TWorld : struct, IWorldType {
        /// <inheritdoc/>
        public override void RegisterTypes(World<TWorld>.TypeRegistrar types) {
            base.RegisterTypes(types);
            types.Component<AstarAIComponent>();
        }
    }
}
