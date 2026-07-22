namespace UniGame.StaticEcs.Features
{
    /// <summary>
    /// Coarse-grained category of a step config used for observability events and for
    /// progression-system dispatch between leaf and composite branches. The exact step
    /// behaviour is selected by the concrete <see cref="IAbilityStepConfig"/> Type, not by this enum.
    /// </summary>
    public enum AbilityStepKind : byte
    {
        Wait = 0,
        ApplyDamage = 1,
        ApplyEffect = 2,
        AoeQuery = 3,
        SetPrimaryTargetFromAoe = 4,
        Custom = 50,
        Sequence = 100,
        Parallel = 101,
        Conditional = 102,
        Repeat = 103,
        LaunchAbility = 200,
    }
}
