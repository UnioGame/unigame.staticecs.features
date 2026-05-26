namespace unigame.staticecs.features {
    /// <summary>
    /// Terminal status reported by a leaf activator and propagated up the composite stack.
    /// <see cref="Running"/> is only valid as the result of <see cref="IAbilityStepActivator{TWorld}.Activate"/>;
    /// it never appears in a completion event.
    /// </summary>
    public enum StepStatus : byte {
        Running = 0,
        Success = 1,
        Failed = 2,
    }
}
