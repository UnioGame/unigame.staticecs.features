using System;
using FFS.Libraries.StaticEcs;

namespace unigame.staticecs.features {
    [Serializable]
    public sealed class AoeNonEmptyCondition : IAbilityStepCondition {
        public bool Evaluate<TWorld>(in AbilityStepConditionContext<TWorld> ctx)
            where TWorld : struct, IWorldType {
            if (!ctx.CastEntity.TryUnpack<TWorld>(out var castEntity)) {
                return false;
            }
            if (!castEntity.Has<World<TWorld>.Multi<AbilityAoeBufferEntry>>()) {
                return false;
            }

            return castEntity.Read<World<TWorld>.Multi<AbilityAoeBufferEntry>>().Length > 0;
        }
    }
}
