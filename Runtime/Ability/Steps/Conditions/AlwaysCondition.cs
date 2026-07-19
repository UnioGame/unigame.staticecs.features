using System;
using UnityEngine.Scripting.APIUpdating;

namespace UniGame.StaticEcs.Features {
    [Serializable]
    [MovedFrom(true, sourceNamespace: "unigame.staticecs.features", sourceAssembly: "unigame.staticecs.features")]
    public sealed class AlwaysCondition : IAbilityStepCondition {
        public bool Evaluate<TWorld>(in AbilityStepConditionContext<TWorld> ctx)
            where TWorld : struct, FFS.Libraries.StaticEcs.IWorldType {
            return true;
        }
    }
}
