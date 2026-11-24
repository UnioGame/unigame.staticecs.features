namespace UniGame.StaticEcs.Features
{
    using FFS.Libraries.StaticEcs;
    using UniGame.StaticEcs.Unity;

    /// <summary>Reads and cancels the dash state of an entity.</summary>
    public static class DashOperations
    {
        public static bool IsPending<TWorld>(EntityGID target)
            where TWorld : struct, IWorldType
        {
            return target.TryUnpack<TWorld>(out var entity) && entity.Has<DashComponent>();
        }

        public static void Cancel<TWorld>(EntityGID target)
            where TWorld : struct, IWorldType
        {
            if (target.TryUnpack<TWorld>(out var entity) && entity.Has<DashComponent>())
                entity.Delete<DashComponent>();
        }

        // --- Main-default overloads ---

        public static bool IsPending(EntityGID target) => IsPending<Main>(target);

        public static void Cancel(EntityGID target) => Cancel<Main>(target);
    }
}
