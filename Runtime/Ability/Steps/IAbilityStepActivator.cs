namespace UniGame.StaticEcs.Features
{
    using FFS.Libraries.StaticEcs;

    /// <summary>
    /// Stateless singleton that activates a leaf step config. Composite steps are handled
    /// directly by <c>AbilityStepProgressionSystem</c> and do not need an activator.
    ///
    /// <para>
    /// <see cref="Activate"/> is called once when the leaf becomes the current step. It returns
    /// <see cref="StepStatus.Running"/> for asynchronous leaves (timer / sub-cast / AoE-broadcast)
    /// — a paired ECS state component + system observes the world and writes
    /// <see cref="StepStatus"/> + <c>AbilityStepReadyTag</c> on the cast-entity when done. For
    /// synchronous leaves (apply damage, apply effect) <see cref="Activate"/> may return
    /// <see cref="StepStatus.Success"/> / <see cref="StepStatus.Failed"/> directly and the
    /// progression system advances to the next leaf in the same tick.
    /// </para>
    ///
    /// Implementations must be allocation-free per call.
    /// </summary>
    public interface IAbilityStepActivator<TWorld>
        where TWorld : struct, IWorldType
    {
        StepStatus Activate(IAbilityStepConfig config, in AbilityStepActivationContext<TWorld> ctx);
        void Cancel(IAbilityStepConfig config, in AbilityStepCancelContext<TWorld> ctx);
    }
}
