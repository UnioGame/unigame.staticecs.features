namespace UniGame.StaticEcs.Features
{
    using FFS.Libraries.StaticEcs;

    /// <summary>
    /// Time-bounded dash slice. It keeps the dash state only; the project movement carries the displacement out.
    /// One dash at a time: stacks stay at 1 and a reapplication is rejected by the handler.
    /// </summary>
    public class DashEffectFeature<TWorld> : EffectFeature<TWorld, DashEffect>
        where TWorld : struct, IWorldType
    {
        protected override IEffectHandler<TWorld, DashEffect> CreateDefaultHandler()
        {
            return new DashEffectHandler<TWorld>();
        }
    }
}
