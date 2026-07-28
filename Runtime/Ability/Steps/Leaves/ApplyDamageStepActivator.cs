namespace UniGame.StaticEcs.Features
{
    using FFS.Libraries.StaticEcs;

    /// <summary>
    /// Stateless activator for <see cref="ApplyDamageStepConfig"/>. Synchronous: routes the
    /// configured amount to <see cref="DamageOperations.RaiseDamage{TWorld}"/> /
    /// <see cref="DamageOperations.RaiseHealing{TWorld}"/> and reports
    /// <see cref="StepStatus.Success"/> in the same tick.
    /// </summary>
    public class ApplyDamageStepActivator<TWorld>
        : AbilityStepActivatorBase<ApplyDamageStepConfig, TWorld>
        where TWorld : struct, IWorldType
    {
        protected override StepStatus OnActivate(
            ApplyDamageStepConfig config,
            in AbilityStepActivationContext<TWorld> ctx
        )
        {
            if (config.Amount <= 0f)
                return StepStatus.Success;

            switch (config.Mode)
            {
                case AbilityTargetMode.Self:
                    return Raise(config, ctx.Caster, ctx.Caster);
                case AbilityTargetMode.PrimaryTarget:
                    return Raise(config, ctx.Caster, ctx.PrimaryTarget);
                case AbilityTargetMode.AoeBroadcast:
                    return RaiseAoe(config, in ctx);
                default:
                    return StepStatus.Failed;
            }
        }

        private static StepStatus RaiseAoe(
            ApplyDamageStepConfig config,
            in AbilityStepActivationContext<TWorld> ctx
        )
        {
            if (!ctx.CastEntity.TryUnpack<TWorld>(out var castEntity))
                return StepStatus.Failed;
            if (!castEntity.Has<World<TWorld>.Multi<AbilityAoeTargetComponent>>())
                return StepStatus.Success;

            ref readonly var entries =
                ref castEntity.Read<World<TWorld>.Multi<AbilityAoeTargetComponent>>();
            for (var i = 0; i < entries.Length; i++)
            {
                var target = entries.Get(i).Target;
                if (config.ExcludeCaster && target.Equals(ctx.Caster))
                    continue;
                Raise(config, ctx.Caster, target);
            }

            return StepStatus.Success;
        }

        private static StepStatus Raise(
            ApplyDamageStepConfig config,
            EntityGID source,
            EntityGID target
        )
        {
            if (!target.TryUnpack<TWorld>(out _))
                return StepStatus.Failed;

            if (config.Type == DamageType.Healing)
                DamageOperations.RaiseHealing<TWorld>(source, target, config.Amount);
            else
                DamageOperations.RaiseDamage<TWorld>(source, target, config.Amount, config.Type);

            return StepStatus.Success;
        }
    }
}
