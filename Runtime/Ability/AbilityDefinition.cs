namespace UniGame.StaticEcs.Features
{
    /// <summary>
    /// Slim authoring data for an ability. Holds only ability-internal fields; cooldown,
    /// resource cost, level requirements and other business rules live in dedicated
    /// configs read by the business layer keyed by <see cref="Id"/> (see plan §1b for the
    /// layering rationale).
    /// </summary>
    public sealed class AbilityDefinition
    {
        public AbilityId Id;
        public bool IsChannel;
        public string DisplayName;

        public AbilityDefinition() { }

        public AbilityDefinition(AbilityId id, bool isChannel = false, string displayName = null)
        {
            Id = id;
            IsChannel = isChannel;
            DisplayName = displayName;
        }
    }
}
