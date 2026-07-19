namespace UniGame.StaticEcs.Features {
    public interface IAbilityStepCondition {
        bool Evaluate<TWorld>(in AbilityStepConditionContext<TWorld> ctx)
            where TWorld : struct, FFS.Libraries.StaticEcs.IWorldType;
    }
}
