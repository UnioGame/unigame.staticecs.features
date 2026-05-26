using FFS.Libraries.StaticEcs;

namespace unigame.staticecs.features {
    /// <summary>
    /// Strongly-typed base for a step activator. Casts the polymorphic
    /// <see cref="IAbilityStepConfig"/> argument to <typeparamref name="TConfig"/> once and
    /// delegates to typed overrides. Allocation-free.
    /// </summary>
    public abstract class AbilityStepActivatorBase<TConfig, TWorld> : IAbilityStepActivator<TWorld>
        where TConfig : class, IAbilityStepConfig
        where TWorld : struct, IWorldType {
        public StepStatus Activate(IAbilityStepConfig config, in AbilityStepActivationContext<TWorld> ctx) {
            return OnActivate((TConfig)config, in ctx);
        }

        public void Cancel(IAbilityStepConfig config, in AbilityStepCancelContext<TWorld> ctx) {
            OnCancel((TConfig)config, in ctx);
        }

        protected abstract StepStatus OnActivate(TConfig config, in AbilityStepActivationContext<TWorld> ctx);

        protected virtual void OnCancel(TConfig config, in AbilityStepCancelContext<TWorld> ctx) { }
    }
}
