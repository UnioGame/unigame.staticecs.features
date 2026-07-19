using System;
using FFS.Libraries.StaticEcs;
using UnityEngine.Scripting.APIUpdating;

namespace UniGame.StaticEcs.Features {
    [Serializable]
    [MovedFrom(true, sourceNamespace: "unigame.staticecs.features", sourceAssembly: "unigame.staticecs.features")]
    public sealed class PrimaryTargetAliveCondition : IAbilityStepCondition {
        public bool Evaluate<TWorld>(in AbilityStepConditionContext<TWorld> ctx)
            where TWorld : struct, IWorldType {
            return ctx.PrimaryTarget.TryUnpack<TWorld>(out _);
        }
    }
}
