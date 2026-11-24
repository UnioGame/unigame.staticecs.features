namespace UniGame.StaticEcs.Features
{
    using Cysharp.Threading.Tasks;
    using FFS.Libraries.StaticEcs;
    using Modifiers;
    using UniGame.Core.Runtime;

    public class StunFeature<TWorld> : StaticEcsFeature<TWorld>
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
            ModifierFlagCache<TWorld, StunSourceComponent>.EnsureRegistered(
                registry,
                (ulong)CharacteristicFlag.Stun,
                static (src, tgt) => StunOperations.RemoveSource<TWorld>(tgt, src));
            return UniTask.CompletedTask;
        }
    }
}
