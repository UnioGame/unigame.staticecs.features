namespace UniGame.StaticEcs.Features
{
    using FFS.Libraries.StaticEcs;
    using Unity;

    public static class ImmobilizeOperations
    {
        public static bool IsActive<TWorld>(EntityGID target)
            where TWorld : struct, IWorldType
        {
            if (!target.TryUnpack<TWorld>(out var entity))
                return false;

            return entity.Has<ImmobilizeActiveTag>();
        }

        public static int SourceCount<TWorld>(EntityGID target)
            where TWorld : struct, IWorldType
        {
            if (!target.TryUnpack<TWorld>(out var entity))
                return 0;

            if (!entity.Has<World<TWorld>.Multi<ImmobilizeSourceComponent>>())
                return 0;

            return entity.Read<World<TWorld>.Multi<ImmobilizeSourceComponent>>().Length;
        }

        public static int AddSource<TWorld>(EntityGID target, EntityGID source)
            where TWorld : struct, IWorldType
        {
            if (!target.TryUnpack<TWorld>(out var entity))
                return 0;

            entity.Add<World<TWorld>.Multi<ImmobilizeSourceComponent>>();

            ref var sources = ref entity.Ref<World<TWorld>.Multi<ImmobilizeSourceComponent>>();
            var previous = sources.Length;

            sources.Add(new ImmobilizeSourceComponent { Source = source });
            var next = sources.Length;

            if (previous == 0 && next > 0)
                entity.Set<ImmobilizeActiveTag>();

            ModifierBackRefRegistrar.Register<TWorld>(source, target, CharacteristicFlag.Immobilize);
            SendChanged<TWorld>(target, previous, next);
            return next;
        }

        public static int RemoveSource<TWorld>(EntityGID target, EntityGID source)
            where TWorld : struct, IWorldType
        {
            if (!target.TryUnpack<TWorld>(out var entity))
                return 0;

            if (!entity.Has<World<TWorld>.Multi<ImmobilizeSourceComponent>>())
                return 0;

            ref var sources = ref entity.Ref<World<TWorld>.Multi<ImmobilizeSourceComponent>>();
            var previous = sources.Length;
            if (previous == 0)
                return 0;

            for (var i = 0; i < sources.Length; i++)
            {
                if (sources[i].Source.Equals(source))
                {
                    sources.RemoveAtSwap(i);
                    break;
                }
            }

            var next = sources.Length;
            if (previous == next)
                return next;

            if (previous > 0 && next == 0 && entity.Has<ImmobilizeActiveTag>())
                entity.Delete<ImmobilizeActiveTag>();

            SendChanged<TWorld>(target, previous, next);
            return next;
        }

        public static bool Clear<TWorld>(EntityGID target)
            where TWorld : struct, IWorldType
        {
            if (!target.TryUnpack<TWorld>(out var entity))
                return false;

            if (!entity.Has<World<TWorld>.Multi<ImmobilizeSourceComponent>>())
                return false;

            ref var sources = ref entity.Ref<World<TWorld>.Multi<ImmobilizeSourceComponent>>();
            var previous = sources.Length;
            if (previous == 0)
                return false;

            sources.Clear();

            if (entity.Has<ImmobilizeActiveTag>())
                entity.Delete<ImmobilizeActiveTag>();

            SendChanged<TWorld>(target, previous, 0);
            return true;
        }

        private static void SendChanged<TWorld>(EntityGID target, int previous, int next)
            where TWorld : struct, IWorldType
        {
            World<TWorld>.SendEvent(
                new ImmobilizeChangedEvent
                {
                    Target = target,
                    PreviousSourceCount = previous,
                    SourceCount = next,
                    BecameActive = previous == 0 && next > 0,
                    BecameInactive = previous > 0 && next == 0,
                }
            );
        }

        // --- Main-default overloads ---

        public static bool IsActive(EntityGID target) => IsActive<Main>(target);

        public static int SourceCount(EntityGID target) => SourceCount<Main>(target);

        public static int AddSource(EntityGID target, EntityGID source) =>
            AddSource<Main>(target, source);

        public static int RemoveSource(EntityGID target, EntityGID source) =>
            RemoveSource<Main>(target, source);

        public static bool Clear(EntityGID target) => Clear<Main>(target);
    }
}
