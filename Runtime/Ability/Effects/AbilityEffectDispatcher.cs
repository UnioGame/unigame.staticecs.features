namespace UniGame.StaticEcs.Features
{
    using FFS.Libraries.StaticEcs;

    public delegate bool AbilityEffectDispatcher<TWorld>(
        EntityGID source,
        EntityGID target,
        float duration,
        float period,
        float delay,
        float magnitude
    )
        where TWorld : struct, IWorldType;
}
