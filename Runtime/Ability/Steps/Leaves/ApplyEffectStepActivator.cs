namespace UniGame.StaticEcs.Features
{
    using FFS.Libraries.StaticEcs;

    public sealed class ApplyEffectStepActivator<TWorld>
        : AbilityStepActivatorBase<ApplyEffectStepConfig, TWorld>
        where TWorld : struct, IWorldType
    {
        protected override StepStatus OnActivate(
            ApplyEffectStepConfig config,
            in AbilityStepActivationContext<TWorld> ctx
        )
        {
            if (!World<TWorld>.HasResource<AbilityEffectDispatchRegistry<TWorld>>())
            {
                return StepStatus.Failed;
            }

            switch (config.Mode)
            {
                case AbilityTargetMode.Self:
                    return Dispatch(config, ctx.Caster, ctx.Caster);
                case AbilityTargetMode.PrimaryTarget:
                    return Dispatch(config, ctx.Caster, ctx.PrimaryTarget);
                case AbilityTargetMode.AoeBroadcast:
                    return DispatchAoe(config, in ctx);
                default:
                    return StepStatus.Failed;
            }
        }

        private static StepStatus DispatchAoe(
            ApplyEffectStepConfig config,
            in AbilityStepActivationContext<TWorld> ctx
        )
        {
            if (!ctx.CastEntity.TryUnpack<TWorld>(out var castEntity))
            {
                return StepStatus.Failed;
            }
            if (!castEntity.Has<World<TWorld>.Multi<AbilityAoeTargetComponent>>())
            {
                return StepStatus.Success;
            }

            ref readonly var entries =
                ref castEntity.Read<World<TWorld>.Multi<AbilityAoeTargetComponent>>();
            for (var i = 0; i < entries.Length; i++)
            {
                var target = entries.Get(i).Target;
                if (config.ExcludeCaster && target.Equals(ctx.Caster))
                {
                    continue;
                }

                Dispatch(config, ctx.Caster, target);
            }

            return StepStatus.Success;
        }

        private static StepStatus Dispatch(
            ApplyEffectStepConfig config,
            EntityGID source,
            EntityGID target
        )
        {
            if (!target.TryUnpack<TWorld>(out _))
            {
                return StepStatus.Failed;
            }

            var registry = World<TWorld>.GetResource<AbilityEffectDispatchRegistry<TWorld>>();
            return registry.TryDispatch(
                config.EffectId,
                source,
                target,
                config.Duration,
                config.Period,
                config.Delay,
                config.Magnitude
            )
                ? StepStatus.Success
                : StepStatus.Failed;
        }
    }
}
