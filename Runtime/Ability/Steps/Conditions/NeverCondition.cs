namespace UniGame.StaticEcs.Features
{
    using System;
    using UnityEngine.Scripting.APIUpdating;

    [Serializable]
    [MovedFrom(
        true,
        sourceNamespace: "unigame.staticecs.features",
        sourceAssembly: "unigame.staticecs.features"
    )]
    public sealed class NeverCondition : IAbilityStepCondition
    {
        public bool Evaluate<TWorld>(in AbilityStepConditionContext<TWorld> ctx)
            where TWorld : struct, FFS.Libraries.StaticEcs.IWorldType
        {
            return false;
        }
    }
}
