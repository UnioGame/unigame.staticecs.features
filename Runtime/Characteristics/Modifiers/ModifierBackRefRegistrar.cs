namespace UniGame.StaticEcs.Features
{
    using FFS.Libraries.StaticEcs;
    using Unity;

    public static class ModifierBackRefRegistrar
    {
        public static void Register<TWorld>(
            EntityGID source,
            EntityGID target,
            CharacteristicFlag flag
        )
            where TWorld : struct, IWorldType
        {
            if (flag == CharacteristicFlag.None)
                return;

            if (!source.TryUnpack<TWorld>(out var src))
                return;

            if (!src.Has<ModifierTrackerComponent>())
                src.Add<ModifierTrackerComponent>();

            if (!src.Has<World<TWorld>.Multi<ModifierTargetComponent>>())
                src.Add<World<TWorld>.Multi<ModifierTargetComponent>>();

            ref var refs = ref src.Ref<World<TWorld>.Multi<ModifierTargetComponent>>();
            var compactTarget = (EntityGIDCompact)target;

            for (var i = 0; i < refs.Length; i++)
            {
                ref var entry = ref refs[i];
                if (entry.Target.Equals(compactTarget))
                {
                    entry.StatMask |= flag;
                    return;
                }
            }

            refs.Add(new ModifierTargetComponent { Target = compactTarget, StatMask = flag });
        }

        // --- Main-default overloads ---

        public static void Register(EntityGID source, EntityGID target, CharacteristicFlag flag) =>
            Register<Main>(source, target, flag);
    }
}
