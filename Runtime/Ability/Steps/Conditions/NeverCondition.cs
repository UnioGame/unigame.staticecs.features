using System;

namespace unigame.staticecs.features {
    [Serializable]
    public sealed class NeverCondition : IAbilityStepCondition {
        public bool Evaluate<TWorld>(in AbilityStepConditionContext<TWorld> ctx)
            where TWorld : struct, FFS.Libraries.StaticEcs.IWorldType {
            return false;
        }
    }
}
