namespace UniGame.StaticEcs.Features
{
    using FFS.Libraries.StaticEcs;
    using UniGame.StaticEcs.Unity;

    /// <summary>Controls a count-up timer and reports completion at its limit.</summary>
    public static class TimerOperations
    {
        public static bool Start<TWorld>(EntityGID target) where TWorld : struct, IWorldType
        {
            if (!target.TryUnpack<TWorld>(out var entity) ||
                !entity.Has<TimerComponent>())
                return false;

            var timer = entity.Read<TimerComponent>();
            if (entity.Has<TimerCompleteTag>())
                return false;
            if (timer.LimitMilliseconds > 0u &&
                timer.ElapsedMilliseconds >= timer.LimitMilliseconds)
                return false;
            if (entity.Has<TimerActiveTag>())
                return true;

            entity.Set<TimerActiveTag>();
            return true;
        }

        public static bool Pause<TWorld>(EntityGID target) where TWorld : struct, IWorldType
        {
            if (!target.TryUnpack<TWorld>(out var entity) ||
                !entity.Has<TimerComponent>())
                return false;
            if (entity.Has<TimerActiveTag>())
                entity.Delete<TimerActiveTag>();
            return true;
        }

        public static bool Reset<TWorld>(EntityGID target) where TWorld : struct, IWorldType
        {
            if (!target.TryUnpack<TWorld>(out var entity) ||
                !entity.Has<TimerComponent>())
                return false;

            ref var timer = ref entity.Mut<TimerComponent>();
            timer.ElapsedMilliseconds = 0u;
            timer.FractionalMilliseconds = 0f;
            entity.Delete<TimerActiveTag>();
            entity.Delete<TimerCompleteTag>();
            return true;
        }

        public static bool Restart<TWorld>(EntityGID target) where TWorld : struct, IWorldType
        {
            if (!target.TryUnpack<TWorld>(out var entity) ||
                !entity.Has<TimerComponent>())
                return false;
            Reset<TWorld>(target);
            return Start<TWorld>(target);
        }

        public static bool Start(EntityGID target) => Start<Main>(target);
        public static bool Pause(EntityGID target) => Pause<Main>(target);
        public static bool Reset(EntityGID target) => Reset<Main>(target);
        public static bool Restart(EntityGID target) => Restart<Main>(target);

    }
}
