using System;
using FFS.Libraries.StaticEcs;

namespace unigame.staticecs.features {
    [Serializable]
    public sealed class PrimaryTargetAliveCondition : IAbilityStepCondition {
        public bool Evaluate<TWorld>(in AbilityStepConditionContext<TWorld> ctx)
            where TWorld : struct, IWorldType {
            return ctx.PrimaryTarget.TryUnpack<TWorld>(out _);
        }
    }
}
