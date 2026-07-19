using FFS.Libraries.StaticEcs;

namespace UniGame.StaticEcs.Features {
    public delegate bool AbilityEffectDispatcher<TWorld>(
        EntityGID source,
        EntityGID target,
        float duration,
        float period,
        float delay,
        float magnitude)
        where TWorld : struct, IWorldType;
}
