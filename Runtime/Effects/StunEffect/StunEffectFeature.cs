namespace UniGame.StaticEcs.Features
{
    using FFS.Libraries.StaticEcs;

    /// <summary>
    /// Time-bounded stun slice. Pulls <see cref="StunFeature{TWorld}"/> dependencies (target tag
    /// and multi-source storage) from the project — register <c>StunFeature</c> alongside this
    /// feature. Stacks are clamped to 1 by default since the underlying counter already handles
    /// concurrent sources.
    /// </summary>
    public class StunEffectFeature<TWorld> : EffectFeature<TWorld, StunEffect>
        where TWorld : struct, IWorldType
    {
        public StunEffectFeature(short tickOrder = DefaultTickOrder, bool registerTickSystem = true)
            : base(
                new StunEffectHandler<TWorld>(),
                maxStacks: 1,
                refreshOnReapply: true,
                tickOrder,
                registerTickSystem
            ) { }
    }
}
