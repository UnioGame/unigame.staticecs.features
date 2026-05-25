namespace unigame.staticecs.features {
    /// <summary>
    /// Authoring data for an ability: phase durations and cooldown. POCO held by
    /// <see cref="AbilityRegistry{TWorld}"/>. Mutate cautiously — instances are shared across casts.
    /// </summary>
    public sealed class AbilityDefinition {
        public AbilityId Id;
        public float ChargeDuration;
        public float CastDuration;
        public float RecoverDuration;
        public float Cooldown;

        public AbilityDefinition() { }

        public AbilityDefinition(AbilityId id, float chargeDuration, float castDuration, float recoverDuration, float cooldown) {
            Id = id;
            ChargeDuration = chargeDuration;
            CastDuration = castDuration;
            RecoverDuration = recoverDuration;
            Cooldown = cooldown;
        }
    }
}
