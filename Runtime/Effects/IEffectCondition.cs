namespace UniGame.StaticEcs.Features
{
    using FFS.Libraries.StaticEcs;

    /// <summary>
    /// Decides whether a <see cref="EffectActivationMode.Condition"/> effect activates this update.
    /// Registered as a world resource per effect type; evaluated once per effect each tick-system update.
    /// </summary>
    public interface IEffectCondition<TWorld, TEffect> : IResource
        where TWorld : struct, IWorldType
        where TEffect : struct, IEffectType
    {
        bool IsMet(EntityGID target, EntityGID source);
    }
}
