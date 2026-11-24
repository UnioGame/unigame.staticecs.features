namespace UniGame.StaticEcs.Features
{
    using FFS.Libraries.StaticEcs;
    using UniGame.StaticEcs.Unity;

    /// <summary>Starts or restarts one configured entity cooldown.</summary>
    public static class CooldownOperations
    {
        public static bool Restart<TWorld>(EntityGID target)
            where TWorld : struct, IWorldType
        {
            if (!target.TryUnpack<TWorld>(out var entity) || !entity.Has<CooldownComponent>())
                return false;

            var duration = entity.Read<CooldownComponent>();
            entity.Set(new CooldownRemainingComponent
            {
                RemainingMilliseconds = duration.DurationMilliseconds
            });
            entity.Delete<CooldownCompleteTag>();
            entity.Set<CooldownActiveTag>();
            return true;
        }

        public static bool Restart(EntityGID target) => Restart<Main>(target);
    }
}
