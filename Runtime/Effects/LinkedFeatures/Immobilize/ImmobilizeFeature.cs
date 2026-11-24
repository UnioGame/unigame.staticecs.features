namespace UniGame.StaticEcs.Features
{
    using Cysharp.Threading.Tasks;
    using FFS.Libraries.StaticEcs;
    using Modifiers;
    using UniGame.Core.Runtime;

    public class ImmobilizeFeature<TWorld> : StaticEcsFeature<TWorld>
        where TWorld : struct, IWorldType
    {
        /// <inheritdoc />
        public override UniTask InitializeAsync(ILifeTime lifeTime)
        {
            if (!World<TWorld>.HasResource<ModifierRegistry>())
            {
                var modifierRegistry = new ModifierRegistry();
                World<TWorld>.SetResource(modifierRegistry);
            }

            ref var registry = ref World<TWorld>.GetResource<ModifierRegistry>();
            ModifierFlagCache<TWorld, ImmobilizeSourceComponent>.EnsureRegistered(
                registry,
                (ulong)CharacteristicFlag.Immobilize,
                static (src, tgt) => ImmobilizeOperations.RemoveSource<TWorld>(tgt, src));
            return UniTask.CompletedTask;
        }
    }
}
