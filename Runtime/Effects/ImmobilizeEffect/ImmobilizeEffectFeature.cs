namespace UniGame.StaticEcs.Features
{
    using FFS.Libraries.StaticEcs;

    /// <summary>
    /// Time-bounded immobilize slice. Pulls <see cref="ImmobilizeFeature{TWorld}"/> dependencies (target tag
    /// and multi-source storage) from the project — register <c>ImmobilizeFeature</c> alongside this
    /// feature. Stacks are clamped to 1 by default since the underlying counter already handles
    /// concurrent sources.
    /// </summary>
    public class ImmobilizeEffectFeature<TWorld> : EffectFeature<TWorld, ImmobilizeEffect>
        where TWorld : struct, IWorldType
    {
        protected override IEffectHandler<TWorld, ImmobilizeEffect> CreateDefaultHandler()
        {
            return new ImmobilizeEffectHandler<TWorld>();
        }
    }
}
