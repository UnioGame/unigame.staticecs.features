namespace UniGame.StaticEcs.Features
{
    using Cysharp.Threading.Tasks;
    using FFS.Libraries.StaticEcs;
    using UniGame.Core.Runtime;

    /// <summary>Marks a world with the shared effects lifecycle infrastructure.</summary>
    public struct EffectsCoreResource : IResource
    {
    }

    /// <summary>Registers shared effect tracking types and registries once per world.</summary>
    public class EffectsCoreFeature<TWorld> : StaticEcsFeature<TWorld>
        where TWorld : struct, IWorldType
    {
        /// <inheritdoc />
        public override UniTask InitializeAsync(ILifeTime lifeTime)
        {
            var core = new EffectsCoreResource();
            var ids = new EffectIdRegistry();
            var effects = new EffectRegistry();

            World<TWorld>.SetResource(core);
            World<TWorld>.SetResource(ids);
            World<TWorld>.SetResource(effects);
            return UniTask.CompletedTask;
        }
    }
}
