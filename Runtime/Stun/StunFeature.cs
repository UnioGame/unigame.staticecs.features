namespace UniGame.StaticEcs.Features
{
    using FFS.Libraries.StaticEcs;
    using Modifiers;

    public class StunFeature<TWorld> : StaticEcsFeature<TWorld>
        where TWorld : struct, IWorldType
    {
        public override void RegisterTypes(World<TWorld>.TypeRegistrar types)
        {
            types.Tag<StunActiveTag>().Multi<StunSourceComponent>().Event<StunChangedEvent>();

            if (!World<TWorld>.HasResource<ModifierRegistry>())
            {
                World<TWorld>.SetResource(new ModifierRegistry());
            }

            ref var registry = ref World<TWorld>.GetResource<ModifierRegistry>();
            ModifierFlagCache<TWorld, StunSourceComponent>.EnsureRegistered(
                registry,
                (ulong)CharacteristicFlag.Stun,
                static (src, tgt) => StunOperations.RemoveSource<TWorld>(tgt, src)
            );
        }
    }
}
