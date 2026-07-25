namespace UniGame.StaticEcs.Features
{
    using Cysharp.Threading.Tasks;
    using FFS.Libraries.StaticEcs;
    using UniGame.Core.Runtime;

    /// <summary>
    /// Registers <see cref="MovementDestinationComponent"/> with the world.
    /// Add <see cref="NavMeshMovementSystem{TWorld}"/> or <c>AstarMovementSystem&lt;TWorld&gt;</c>
    /// to the update group to drive actual navigation.
    /// </summary>
    public class MovementFeature<TWorld> : StaticEcsFeature<TWorld>
        where TWorld : struct, IWorldType
    {
        /// <inheritdoc/>
        public override UniTask InitializeAsync(ILifeTime lifeTime)
        {
            return UniTask.CompletedTask;
        }
    }
}
