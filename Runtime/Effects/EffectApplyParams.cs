namespace UniGame.StaticEcs.Features
{
    /// <summary>
    /// Per-application overrides of a status effect's defaults. A zero value selects the default from
    /// <see cref="EffectConfig{TWorld,TEffect}"/>; values outside the type's caps are clamped to them.
    /// </summary>
    public struct EffectApplyParams
    {
        /// <summary>Seconds of lifetime; used by <see cref="EffectDurationMode.Time"/> effects.</summary>
        public float Duration;

        /// <summary>Activations of lifetime; used by <see cref="EffectDurationMode.Charges"/> effects.</summary>
        public int Charges;

        /// <summary>Seconds between activations; used by <see cref="EffectActivationMode.CustomInterval"/> effects.</summary>
        public float Period;

        /// <summary>Strength handed to the handler, for example damage per activation. Kept on reapply.</summary>
        public float Magnitude;

        /// <summary>Seconds before the effect becomes active.</summary>
        public float Delay;

        /// <summary>Readable name of the source captured for history; the source may die before the effect ends.</summary>
        public string SourceName;
    }
}
