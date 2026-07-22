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
        public HealOverTimeFeature(
            int maxStacks = 5,
            bool refreshOnReapply = true,
            short tickOrder = DefaultTickOrder,
            bool registerTickSystem = true
        )
            : base(
                new HealOverTimeHandler<TWorld>(),
                maxStacks,
                refreshOnReapply,
                tickOrder,
                registerTickSystem
            ) { }

        public override void RegisterTypes(World<TWorld>.TypeRegistrar types)
        {
            types.Component<HealOverTimeComponent>();
            base.RegisterTypes(types);
        }
    }
}
