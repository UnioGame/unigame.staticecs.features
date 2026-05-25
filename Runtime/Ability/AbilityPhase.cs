namespace unigame.staticecs.features {
    /// <summary>
    /// Cast phase of an active ability. The Idle state is represented by absence of
    /// <see cref="AbilityCastComponent"/> on the caster, so this enum only enumerates active phases.
    /// </summary>
    public enum AbilityPhase : byte {
        Charging = 0,
        Casting = 1,
        Recovering = 2,
    }
}
