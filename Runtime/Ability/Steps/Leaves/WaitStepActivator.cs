namespace UniGame.StaticEcs.Features
{
    using FFS.Libraries.StaticEcs;

    /// <summary>
    /// Stateless activator for <see cref="WaitStepConfig"/>. For positive durations attaches
    /// <see cref="AbilityWaitComponent"/> to the cast-entity and returns Running; the paired
    /// <see cref="AbilityWaitSystem{TWorld}"/> drains the timer and signals completion via
    /// <see cref="AbilityStepReadyTag"/> + <see cref="AbilityStepStatusComponent"/>. Zero durations
    /// resolve synchronously inside <see cref="OnActivate"/>.
    /// </summary>
    public class WaitStepActivator<TWorld> : AbilityStepActivatorBase<WaitStepConfig, TWorld>
        where TWorld : struct, IWorldType
    {
        protected override StepStatus OnActivate(
            WaitStepConfig config,
            in AbilityStepActivationContext<TWorld> ctx
        )
        {
            if (config.Duration <= 0f)
                return StepStatus.Success;
            if (!ctx.CastEntity.TryUnpack<TWorld>(out var entity))
                return StepStatus.Failed;
            entity.Set(new AbilityWaitComponent { TimeLeft = config.Duration });
            return StepStatus.Running;
        }

        protected override void OnCancel(
            WaitStepConfig config,
            in AbilityStepCancelContext<TWorld> ctx
        )
        {
            if (
                ctx.CastEntity.TryUnpack<TWorld>(out var entity)
                && entity.Has<AbilityWaitComponent>()
            )
                entity.Delete<AbilityWaitComponent>();
        }
    }
}
