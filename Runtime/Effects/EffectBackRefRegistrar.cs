namespace UniGame.StaticEcs.Features
{
    using FFS.Libraries.StaticEcs;
    using Unity;

    /// <summary>
    /// Installs <see cref="EffectTargetComponent"/> entries on the source entity for a given target +
    /// <see cref="EffectFlag"/>. Mirrors <c>ModifierBackRefRegistrar</c>: existing entries
    /// merge their mask via OR; new entries push to the back-ref multi-component.
    /// </summary>
    public static class EffectBackRefRegistrar
    {
        public static void Register<TWorld>(EntityGID source, EntityGID target, EffectFlag flag)
            where TWorld : struct, IWorldType
        {
            if (flag == EffectFlag.None)
                return;

            if (!source.TryUnpack<TWorld>(out var src))
                return;

            if (!src.Has<EffectTrackerComponent>())
                src.Add<EffectTrackerComponent>();

            if (!src.Has<World<TWorld>.Multi<EffectTargetComponent>>())
                src.Add<World<TWorld>.Multi<EffectTargetComponent>>();

            ref var refs = ref src.Ref<World<TWorld>.Multi<EffectTargetComponent>>();
            var compactTarget = (EntityGIDCompact)target;

            for (var i = 0; i < refs.Length; i++)
            {
                ref var entry = ref refs[i];
                if (entry.Target.Equals(compactTarget))
                {
                    entry.Mask |= flag;
                    return;
                }
            }

            refs.Add(new EffectTargetComponent { Target = compactTarget, Mask = flag });
        }

        /// <summary>
        /// Removes the matching back-ref entry / flag bit. Safe to call during the source's own
        /// destroy hook: if the source is not <see cref="GIDStatus.Active"/>, this method is a
        /// noop, since the source's back-ref multi is about to be cleaned up wholesale.
        /// </summary>
        public static void Unregister<TWorld>(EntityGID source, EntityGID target, EffectFlag flag)
            where TWorld : struct, IWorldType
        {
            if (flag == EffectFlag.None)
                return;

            if (source.Status<TWorld>() != GIDStatus.Active)
                return;

            if (!source.TryUnpack<TWorld>(out var src))
                return;

            if (!src.Has<World<TWorld>.Multi<EffectTargetComponent>>())
                return;

            ref var refs = ref src.Ref<World<TWorld>.Multi<EffectTargetComponent>>();
            var compactTarget = (EntityGIDCompact)target;

            for (var i = 0; i < refs.Length; i++)
            {
                ref var entry = ref refs[i];
                if (entry.Target.Equals(compactTarget))
                {
                    entry.Mask &= ~flag;
                    if (entry.Mask == EffectFlag.None)
                        refs.RemoveAtSwap(i);

                    return;
                }
            }
        }

        // --- Main-default overloads ---

        public static void Register(EntityGID source, EntityGID target, EffectFlag flag) =>
            Register<Main>(source, target, flag);

        public static void Unregister(EntityGID source, EntityGID target, EffectFlag flag) =>
            Unregister<Main>(source, target, flag);
    }
}
