namespace UniGame.StaticEcs.Features
{
    using FFS.Libraries.StaticEcs;
    using Unity;

    /// <summary>
    /// Convenience entry point: stamps <see cref="ModificationEffectComponent{TStat}"/> on the target
    /// and forwards to <c>EffectOperations.Apply</c> for <see cref="ModificationEffect{TStat}"/>.
    /// </summary>
    public static class ModificationEffectOperations
    {
        public static bool Apply<TWorld, TStat>(
            EntityGID target,
            EntityGID source,
            CharacteristicModifierOp op,
            float value,
            float duration
        )
            where TWorld : struct, IWorldType
            where TStat : struct, ICharacteristicType
        {
            if (duration <= 0f)
            {
                return false;
            }

            if (!target.TryUnpack<TWorld>(out var entity))
            {
                return false;
            }

            if (!entity.Has<ModificationEffectComponent<TStat>>())
            {
                entity.Add<ModificationEffectComponent<TStat>>();
            }

            ref var data = ref entity.Ref<ModificationEffectComponent<TStat>>();
            data.Op = op;
            data.Value = value;

            return EffectOperations.Apply<TWorld, ModificationEffect<TStat>>(
                target,
                source,
                duration
            );
        }

        // --- Main-default overload ---

        public static bool Apply<TStat>(
            EntityGID target,
            EntityGID source,
            CharacteristicModifierOp op,
            float value,
            float duration
        )
            where TStat : struct, ICharacteristicType =>
            Apply<Main, TStat>(target, source, op, value, duration);
    }
}
