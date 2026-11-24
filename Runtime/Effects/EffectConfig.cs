namespace UniGame.StaticEcs.Features
{
    using FFS.Libraries.StaticEcs;

    /// <summary>
    /// Per-effect stacking and re-apply policy. Set as a world resource by
    /// <c>EffectFeature&lt;TWorld, TEffect&gt;</c> and read by <c>EffectOperations.Apply</c> when
    /// the target already carries the same effect.
    /// </summary>
    public class EffectConfig<TWorld, TEffect> : IResource
        where TWorld : struct, IWorldType
        where TEffect : struct, IEffectType
    {
        /// <summary>Maximum stack count for the effect.</summary>
        public int MaxStacks;

        /// <summary>Whether reapplication refreshes timing.</summary>
        public bool RefreshOnReapply;

        /// <summary>Execution order of the typed effect tick system.</summary>
        public short TickOrder;

        /// <summary>Whether the typed effect tick system is installed.</summary>
        public bool RegisterTickSystem;

        /// <summary>What limits the lifetime of the effect.</summary>
        public EffectDurationMode DurationMode = EffectDurationMode.Time;

        /// <summary>When the effect activates its handler. Legacy effects use their own period.</summary>
        public EffectActivationMode ActivationMode = EffectActivationMode.CustomInterval;

        /// <summary>Whether reapplication adds duration or charges up to the caps instead of refreshing.</summary>
        public bool Accumulate;

        /// <summary>Lifetime in seconds when the application passes none; 0 requires the caller to pass one.</summary>
        public float DefaultDuration;

        /// <summary>Largest accumulated lifetime in seconds; 0 means no cap.</summary>
        public float MaxDuration;

        /// <summary>Lifetime in activations when the application passes none.</summary>
        public int DefaultCharges;

        /// <summary>Largest accumulated charge count; 0 means no cap.</summary>
        public int MaxCharges;

        /// <summary>Seconds between activations when the application passes none.</summary>
        public float DefaultPeriod;

        /// <summary>Whether destroying the source removes the effect from its targets.</summary>
        public bool RemoveWithSource = true;

        /// <summary>Effects of one group exclude each other on a target; empty means no group.</summary>
        public string ConflictGroup = string.Empty;

        /// <summary>Priority inside the conflict group; the higher one wins, an equal one is replaced.</summary>
        public int ConflictPriority;

        /// <summary>
        /// Whether the effect is listed in the roster of its target. An instant effect exists on the target only for the
        /// moment of its application, so it is never listed.
        /// </summary>
        public bool Persists => DurationMode != EffectDurationMode.Instant;

        /// <summary>Creates an effect runtime configuration.</summary>
        public EffectConfig(
            int maxStacks = 1,
            bool refreshOnReapply = true,
            short tickOrder = 200,
            bool registerTickSystem = true)
        {
            MaxStacks = maxStacks < 1 ? 1 : maxStacks;
            RefreshOnReapply = refreshOnReapply;
            TickOrder = tickOrder;
            RegisterTickSystem = registerTickSystem;
        }

        /// <summary>Creates the configuration of an effect that lives only for its application.</summary>
        public static EffectConfig<TWorld, TEffect> CreateInstant()
        {
            var config = new EffectConfig<TWorld, TEffect>(1, false, 0, false);
            config.DurationMode = EffectDurationMode.Instant;
            config.ActivationMode = EffectActivationMode.Tick;
            config.RemoveWithSource = false;
            return config;
        }

        /// <summary>Creates a configuration from authored status defaults.</summary>
        public EffectConfig(
            StatusEffectSettings settings,
            short tickOrder = 200,
            bool registerTickSystem = true)
            : this(1, true, tickOrder, registerTickSystem)
        {
            ApplySettings(settings);
        }

        /// <summary>Copies authored status defaults into the configuration.</summary>
        public void ApplySettings(StatusEffectSettings settings)
        {
            DurationMode = settings.durationMode;
            ActivationMode = settings.activationMode;
            Accumulate = true;
            DefaultDuration = settings.duration;
            MaxDuration = settings.maxDuration;
            DefaultCharges = settings.charges;
            MaxCharges = settings.maxCharges;
            DefaultPeriod = settings.activationMode == EffectActivationMode.Tick
                ? StatusEffectSettings.TickSeconds
                : settings.period;
            RemoveWithSource = settings.removeWithSource;
            ConflictGroup = settings.conflictGroup ?? string.Empty;
            ConflictPriority = settings.conflictPriority;
        }
    }
}
