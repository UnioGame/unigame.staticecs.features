namespace UniGame.StaticEcs.Features
{
    using FFS.Libraries.StaticEcs;

    /// <summary>The stacks and timers of one effect on one target, the part a reapplication merges into.</summary>
    internal struct EffectLifetimeState
    {
        public int Stacks;
        public float TimeLeft;
        public int ChargesLeft;
        public float Period;
        public float PeriodLeft;
        public float DelayLeft;
    }

    /// <summary>Lifetime values of the application being merged; a zero value was already replaced by the default.</summary>
    internal readonly struct EffectLifetimeInput
    {
        public readonly float Duration;
        public readonly int Charges;
        public readonly float Period;
        public readonly float Delay;

        public EffectLifetimeInput(float duration, int charges, float period, float delay)
        {
            Duration = duration;
            Charges = charges;
            Period = period;
            Delay = delay;
        }
    }

    /// <summary>The configuration limits and policy a reapplication follows.</summary>
    internal readonly struct EffectMergeRules
    {
        public readonly int MaxStacks;
        public readonly EffectDurationMode DurationMode;
        public readonly bool Accumulate;
        public readonly bool RefreshOnReapply;
        public readonly float MaxDuration;
        public readonly int MaxCharges;

        public EffectMergeRules(
            int maxStacks,
            EffectDurationMode durationMode,
            bool accumulate,
            bool refreshOnReapply,
            float maxDuration,
            int maxCharges)
        {
            MaxStacks = maxStacks;
            DurationMode = durationMode;
            Accumulate = accumulate;
            RefreshOnReapply = refreshOnReapply;
            MaxDuration = maxDuration;
            MaxCharges = maxCharges;
        }

        /// <param name="accumulate">Whether a reapplication adds to the lifetime instead of refreshing it.</param>
        public static EffectMergeRules From<TWorld, TEffect>(
            EffectConfig<TWorld, TEffect> config,
            bool accumulate)
            where TWorld : struct, IWorldType
            where TEffect : struct, IEffectType
        {
            return new EffectMergeRules(
                config.MaxStacks,
                config.DurationMode,
                accumulate,
                config.RefreshOnReapply,
                config.MaxDuration,
                config.MaxCharges);
        }
    }

    /// <summary>Pure rules for the lifetime of an effect: how a reapplication merges and when the effect is spent.</summary>
    internal static class EffectLifetime
    {
        /// <summary>Absorbs the float drift of accumulated frame deltas when a timer is compared with zero.</summary>
        public const float TimeEpsilon = 1e-4f;

        /// <summary>
        /// Merges one more application into the state of an effect. Without a previous state (no stacks) the effect
        /// starts: one stack and the applied lifetime. Otherwise the application adds a stack up to the cap, then follows
        /// the policy: accumulate adds duration or charges up to their caps and keeps the activation phase; refresh takes
        /// the larger lifetime, restarts the period and, while the effect is still delayed, the delay; with neither the
        /// timers stay as they were.
        /// </summary>
        public static EffectLifetimeState Merge(
            in EffectLifetimeState previous,
            in EffectLifetimeInput applied,
            in EffectMergeRules rules)
        {
            if (previous.Stacks <= 0)
            {
                return new EffectLifetimeState
                {
                    Stacks = 1,
                    TimeLeft = applied.Duration,
                    ChargesLeft = applied.Charges,
                    Period = applied.Period,
                    PeriodLeft = applied.Period > 0f ? applied.Period : 0f,
                    DelayLeft = applied.Delay,
                };
            }

            var next = previous;
            next.Stacks = previous.Stacks + 1;
            if (next.Stacks > rules.MaxStacks)
                next.Stacks = rules.MaxStacks;

            if (rules.Accumulate)
            {
                if (rules.DurationMode == EffectDurationMode.Time)
                {
                    next.TimeLeft = previous.TimeLeft + applied.Duration;
                    if (rules.MaxDuration > 0f && next.TimeLeft > rules.MaxDuration)
                        next.TimeLeft = rules.MaxDuration;
                }
                else
                {
                    next.ChargesLeft = previous.ChargesLeft + applied.Charges;
                    if (rules.MaxCharges > 0 && next.ChargesLeft > rules.MaxCharges)
                        next.ChargesLeft = rules.MaxCharges;
                }
            }
            else if (rules.RefreshOnReapply)
            {
                if (applied.Duration > previous.TimeLeft)
                    next.TimeLeft = applied.Duration;

                if (applied.Charges > previous.ChargesLeft)
                    next.ChargesLeft = applied.Charges;

                next.Period = applied.Period;
                next.PeriodLeft = applied.Period > 0f ? applied.Period : 0f;
                next.DelayLeft = previous.DelayLeft > 0f ? applied.Delay : 0f;
            }

            return next;
        }

        /// <summary>
        /// Returns true when nothing is left of the effect: an instant effect always, a timed one when its time ran out,
        /// a charged one when its charges did.
        /// </summary>
        public static bool IsExhausted(EffectDurationMode mode, float timeLeft, int chargesLeft)
        {
            switch (mode)
            {
                case EffectDurationMode.Instant:
                    return true;
                case EffectDurationMode.Charges:
                    return chargesLeft <= 0;
                default:
                    return timeLeft <= TimeEpsilon;
            }
        }
    }
}
