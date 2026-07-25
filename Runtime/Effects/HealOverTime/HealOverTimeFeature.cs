namespace UniGame.StaticEcs.Features
{
    using FFS.Libraries.StaticEcs;

    /// <summary>
    /// HealOverTime slice. Registers <see cref="HealOverTimeComponent"/> alongside the generic
    /// <see cref="EffectFeature{TWorld, TEffect}"/> for <see cref="HealOverTimeEffect"/>, with
    /// <see cref="HealOverTimeHandler{TWorld}"/> as the default handler.
    /// </summary>
    public class HealOverTimeFeature<TWorld> : EffectFeature<TWorld, HealOverTimeEffect>
        where TWorld : struct, IWorldType
    {
        protected override int DefaultMaxStacks => 5;

        protected override IEffectHandler<TWorld, HealOverTimeEffect> CreateDefaultHandler()
        {
            return new HealOverTimeHandler<TWorld>();
        }
    }
}
