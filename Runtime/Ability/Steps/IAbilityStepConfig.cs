namespace UniGame.StaticEcs.Features
{
    /// <summary>
    /// Marker contract for every step config (leaf or composite). Pure data: held by
    /// <see cref="AbilityRegistry{TWorld}"/>, walked by <c>AbilityStepProgressionSystem</c>,
    /// activated by <see cref="IAbilityStepActivator{TWorld}"/> singletons looked up by config Type.
    /// </summary>
    public interface IAbilityStepConfig
    {
        AbilityStepKind Kind { get; }
        string NodeGuid { get; }
    }
}
