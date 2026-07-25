namespace UniGame.StaticEcs.Features
{
    using Cysharp.Threading.Tasks;
    using FFS.Libraries.StaticEcs;
    using Modifiers;
    using UniGame.Core.Runtime;

    public class ModifierBackRefFeature<TWorld> : StaticEcsFeature<TWorld>
        where TWorld : struct, IWorldType
    {
        /// <inheritdoc />
        public override UniTask InitializeAsync(ILifeTime lifeTime)
        {
            if (!World<TWorld>.HasResource<ModifierRegistry>())
            {
                var registry = new ModifierRegistry();
                World<TWorld>.SetResource(registry);
            }

            return UniTask.CompletedTask;
        }
    }
}
