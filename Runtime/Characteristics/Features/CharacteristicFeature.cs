namespace UniGame.StaticEcs.Features
{
    using FFS.Libraries.StaticEcs;
    using Modifiers;

    public class CharacteristicFeature<TWorld, TCharacteristic> : StaticEcsFeature<TWorld>
        where TWorld : struct, IWorldType
        where TCharacteristic : struct, ICharacteristicType
    {
        public override void RegisterTypes(World<TWorld>.TypeRegistrar types)
        {
            types
                .Component<CharacteristicComponent<TCharacteristic>>()
                .Event<CharacteristicChangedEvent<TCharacteristic>>()
                .Multi<CharacteristicModifierComponent<TCharacteristic>>();

            if (!World<TWorld>.HasResource<ModifierRegistry>())
            {
                World<TWorld>.SetResource(new ModifierRegistry());
            }

            ref var registry = ref World<TWorld>.GetResource<ModifierRegistry>();
            ModifierFlagCache<TWorld, TCharacteristic>.EnsureRegistered(
                registry,
                (ulong)CharacteristicFlagOf<TCharacteristic>.Value,
                static (src, tgt) =>
                    CharacteristicModifierExtensions.RemoveModifiersFromSource<
                        TWorld,
                        TCharacteristic
                    >(tgt, src)
            );
        }
    }
}
