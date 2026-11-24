namespace UniGame.StaticEcs.Features
{
    using FFS.Libraries.StaticEcs;

    /// <summary>
    /// Bridges <see cref="ModificationEffect{TStat}"/> lifecycle to
    /// <c>CharacteristicModifierExtensions</c>. On apply the handler installs a
    /// <c>(Op, Value)</c> modifier keyed by the effect source; on removal it strips every
    /// modifier originating from that source on the matching stat.
    /// </summary>
    public class ModificationEffectHandler<TWorld, TStat>
        : IEffectHandler<TWorld, ModificationEffect<TStat>>
        where TWorld : struct, IWorldType
        where TStat : struct, ICharacteristicType
    {
        public bool CanApply(in EffectApplyContext context) => true;

        public void OnApplied(in EffectApplyContext context)
        {
            if (!context.Target.TryUnpack<TWorld>(out var entity))
                return;

            if (!entity.Has<ModificationEffectComponent<TStat>>())
                return;

            var data = entity.Read<ModificationEffectComponent<TStat>>();
            if (context.PreviousStacks > 0)
                CharacteristicModifierExtensions.RemoveModifiersFromSource<TWorld, TStat>(
                    context.Target,
                    context.Source
                );

            CharacteristicModifierExtensions.ApplyModifier<TWorld, TStat>(
                context.Target,
                context.Source,
                data.Op,
                data.Value
            );
        }

        public void OnTick(EntityGID target, EntityGID source, int stacks) { }

        public void OnRemoved(EntityGID target, EntityGID source, int stacks, bool expired)
        {
            CharacteristicModifierExtensions.RemoveModifiersFromSource<TWorld, TStat>(
                target,
                source
            );
        }
    }
}
