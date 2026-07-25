namespace UniGame.StaticEcs.Features
{
    using System;
    using FFS.Libraries.StaticEcs;
    using Unity;

    public class AoeQueryStepActivator<TWorld>
        : AbilityStepActivatorBase<AoeQueryStepConfig, TWorld>
        where TWorld : struct, IWorldType
    {
        private const int MaxStackTargets = 64;

        protected override StepStatus OnActivate(
            AoeQueryStepConfig config,
            in AbilityStepActivationContext<TWorld> ctx
        )
        {
            if (
                config.Radius <= 0f
                || config.MaxTargets <= 0
                || config.MaxTargets > MaxStackTargets
            )
            {
                return StepStatus.Failed;
            }
            if (
                !ctx.Caster.TryUnpack<TWorld>(out var caster)
                || !ctx.CastEntity.TryUnpack<TWorld>(out var castEntity)
            )
            {
                return StepStatus.Failed;
            }
            if (!caster.Has<TransformComponent>())
            {
                return StepStatus.Failed;
            }

            var binding = caster.Read<TransformComponent>();
            if (binding.Transform == null || !World<TWorld>.HasResource<ITargetIndex<TWorld>>())
            {
                return StepStatus.Failed;
            }

            Span<EntityGID> buffer = stackalloc EntityGID[config.MaxTargets];
            var count = World<TWorld>
                .GetResource<ITargetIndex<TWorld>>()
                .FillNearestSphere(
                    binding.Transform.position,
                    config.Radius,
                    buffer,
                    config.ExcludeCaster ? ctx.Caster : default);

            if (!castEntity.Has<World<TWorld>.Multi<AbilityAoeTargetComponent>>())
            {
                castEntity.Add<World<TWorld>.Multi<AbilityAoeTargetComponent>>();
            }

            ref var entries = ref castEntity.Ref<World<TWorld>.Multi<AbilityAoeTargetComponent>>();
            entries.Clear();
            for (var i = 0; i < count; i++)
            {
                var target = buffer[i];
                entries.Add(new AbilityAoeTargetComponent { Target = target });
            }

            return StepStatus.Success;
        }
    }
}
