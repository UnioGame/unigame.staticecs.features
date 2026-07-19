using System;
using FFS.Libraries.StaticEcs;
using UnityEngine.Scripting.APIUpdating;

namespace UniGame.StaticEcs.Features {
    [Serializable]
    [MovedFrom(true, sourceNamespace: "unigame.staticecs.features", sourceAssembly: "unigame.staticecs.features")]
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
