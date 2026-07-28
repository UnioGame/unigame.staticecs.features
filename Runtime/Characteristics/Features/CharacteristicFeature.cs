namespace UniGame.StaticEcs.Features
{
    using Cysharp.Threading.Tasks;
    using FFS.Libraries.StaticEcs;
    using Modifiers;
    using UniGame.Core.Runtime;

    public class CharacteristicFeature<TWorld, TCharacteristic> :
        StaticEcsFeature<TWorld>
        where TWorld : struct, IWorldType
        where TCharacteristic : struct, ICharacteristicType
    {
        /// <inheritdoc />
        public override UniTask InitializeAsync(ILifeTime lifeTime)
        {
            if (!World<TWorld>.HasResource<ModifierRegistry>())
            {
                var defaultRegistry = new ModifierRegistry();
                World<TWorld>.SetResource(defaultRegistry);
            }

            ref var registry = ref World<TWorld>.GetResource<ModifierRegistry>();
            ModifierFlagCache<TWorld, TCharacteristic>.EnsureRegistered(
                registry,
                (ulong)CharacteristicFlagOf<TCharacteristic>.Value,
                static (src, tgt) =>
                    CharacteristicModifierExtensions.RemoveModifiersFromSource<TWorld, TCharacteristic>(tgt, src));
            return UniTask.CompletedTask;
        }
    }
}
