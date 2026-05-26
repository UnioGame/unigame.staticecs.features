using System;
using FFS.Libraries.StaticEcs;
using unigame.staticecs.unity;

namespace unigame.staticecs.features {
    public sealed class AoeQueryStepActivator<TWorld> : AbilityStepActivatorBase<AoeQueryStepConfig, TWorld>
        where TWorld : struct, IWorldType {
        private const int MaxStackTargets = 64;

        protected override StepStatus OnActivate(AoeQueryStepConfig config, in AbilityStepActivationContext<TWorld> ctx) {
            if (config.Radius <= 0f || config.MaxTargets <= 0 || config.MaxTargets > MaxStackTargets) {
                return StepStatus.Failed;
            }
            if (!ctx.Caster.TryUnpack<TWorld>(out var caster) || !ctx.CastEntity.TryUnpack<TWorld>(out var castEntity)) {
                return StepStatus.Failed;
            }
            if (!caster.Has<TransformBindingComponent>()) {
                return StepStatus.Failed;
            }

            var binding = caster.Read<TransformBindingComponent>();
            if (binding.Transform == null || !World<TWorld>.HasResource<ITargetIndex<TWorld>>()) {
                return StepStatus.Failed;
            }

            Span<EntityGID> buffer = stackalloc EntityGID[config.MaxTargets];
            var count = World<TWorld>.GetResource<ITargetIndex<TWorld>>()
                .FillSphere(binding.Transform.position, config.Radius, buffer);

            if (!castEntity.Has<World<TWorld>.Multi<AbilityAoeBufferEntry>>()) {
                castEntity.Add<World<TWorld>.Multi<AbilityAoeBufferEntry>>();
            }

            ref var entries = ref castEntity.Ref<World<TWorld>.Multi<AbilityAoeBufferEntry>>();
            entries.Clear();
            for (var i = 0; i < count; i++) {
                var target = buffer[i];
                if (config.ExcludeCaster && target.Equals(ctx.Caster)) {
                    continue;
                }

                entries.Add(new AbilityAoeBufferEntry { Target = target });
            }

            return StepStatus.Success;
        }
    }
}
