using FFS.Libraries.StaticEcs;
using UnityEngine;
 

namespace UniGame.StaticEcs.Features {
    using Unity;

    public sealed class SetPrimaryTargetFromAoeStepActivator<TWorld> : AbilityStepActivatorBase<SetPrimaryTargetFromAoeStepConfig, TWorld>
        where TWorld : struct, IWorldType {
        protected override StepStatus OnActivate(SetPrimaryTargetFromAoeStepConfig config, in AbilityStepActivationContext<TWorld> ctx) {
            if (!ctx.CastEntity.TryUnpack<TWorld>(out var castEntity)) {
                return StepStatus.Failed;
            }
            if (!castEntity.Has<World<TWorld>.Multi<AbilityAoeBufferEntry>>()) {
                return StepStatus.Failed;
            }

            ref readonly var entries = ref castEntity.Read<World<TWorld>.Multi<AbilityAoeBufferEntry>>();
            if (entries.Length == 0) {
                return StepStatus.Failed;
            }

            var selected = SelectTarget(config.Selector, in entries, ctx.Caster);
            if (!selected.TryUnpack<TWorld>(out _)) {
                return StepStatus.Failed;
            }

            ref var runtime = ref castEntity.Mut<AbilityCastRuntimeComponent>();
            runtime.PrimaryTarget = selected;
            return StepStatus.Success;
        }

        private static EntityGID SelectTarget(
            AoeTargetSelector selector,
            in World<TWorld>.Multi<AbilityAoeBufferEntry> entries,
            EntityGID caster) {
            switch (selector) {
                case AoeTargetSelector.Random:
                    return SelectRandom(in entries);
                case AoeTargetSelector.Closest:
                    return SelectClosest(in entries, caster);
                default:
                    return entries.Get(0).Target;
            }
        }

        private static EntityGID SelectRandom(in World<TWorld>.Multi<AbilityAoeBufferEntry> entries) {
            if (!World<TWorld>.HasResource<IAbilityRng<TWorld>>()) {
                return entries.Get(0).Target;
            }

            var index = World<TWorld>.GetResource<IAbilityRng<TWorld>>().Range(0, entries.Length);
            if (index < 0) {
                index = 0;
            }
            if (index >= entries.Length) {
                index = entries.Length - 1;
            }

            return entries.Get(index).Target;
        }

        private static EntityGID SelectClosest(in World<TWorld>.Multi<AbilityAoeBufferEntry> entries, EntityGID caster) {
            if (!caster.TryUnpack<TWorld>(out var casterEntity) || !casterEntity.Has<TransformBindingComponent>()) {
                return default;
            }

            var casterBinding = casterEntity.Read<TransformBindingComponent>();
            if (casterBinding.Transform == null) {
                return default;
            }

            var casterPosition = casterBinding.Transform.position;
            var best = default(EntityGID);
            var bestDistance = float.MaxValue;

            for (var i = 0; i < entries.Length; i++) {
                var target = entries.Get(i).Target;
                if (!target.TryUnpack<TWorld>(out var targetEntity) || !targetEntity.Has<TransformBindingComponent>()) {
                    continue;
                }

                var binding = targetEntity.Read<TransformBindingComponent>();
                if (binding.Transform == null) {
                    continue;
                }

                var distance = (binding.Transform.position - casterPosition).sqrMagnitude;
                if (distance < bestDistance) {
                    bestDistance = distance;
                    best = target;
                }
            }

            return best;
        }
    }
}
