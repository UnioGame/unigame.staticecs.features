namespace UniGame.StaticEcs.Features
{
    using System.Diagnostics;
    using FFS.Libraries.StaticEcs;
    using Unity;

    /// <summary>
    /// Public entry points for the effects framework.
    ///
    /// <para>
    /// <see cref="Apply{TWorld,TEffect}"/> creates or refreshes
    /// <see cref="EffectComponent{TEffect}"/> on the target, registers a back-reference on the
    /// source for automatic cleanup, updates the roster, and dispatches the configured handler.
    /// <see cref="Remove{TWorld,TEffect}"/> tears down the effect and raises
    /// <see cref="EffectRemovedEvent{TEffect}"/> with <c>Expired = false</c>.
    /// </para>
    ///
    /// <para>
    /// Group operations <see cref="RemoveAll{TWorld}"/> and
    /// <see cref="RemoveByMask{TWorld}"/> dispatch through <see cref="EffectRegistry"/> using
    /// the per-target roster, so they do not need to know the concrete <c>TEffect</c> type.
    /// </para>
    ///
    /// Project code never mutates <see cref="EffectComponent{TEffect}"/> directly.
    /// </summary>
    public static class EffectOperations
    {
        public static bool Has<TWorld, TEffect>(EntityGID target)
            where TWorld : struct, IWorldType
            where TEffect : struct, IEffectType
        {
            if (!target.TryUnpack<TWorld>(out var entity))
                return false;

            return entity.Has<EffectComponent<TEffect>>();
        }

        public static float GetTimeLeft<TWorld, TEffect>(EntityGID target)
            where TWorld : struct, IWorldType
            where TEffect : struct, IEffectType
        {
            if (!target.TryUnpack<TWorld>(out var entity))
                return 0f;

            if (!entity.Has<EffectComponent<TEffect>>())
                return 0f;

            return entity.Read<EffectComponent<TEffect>>().TimeLeft;
        }

        public static int GetStacks<TWorld, TEffect>(EntityGID target)
            where TWorld : struct, IWorldType
            where TEffect : struct, IEffectType
        {
            if (!target.TryUnpack<TWorld>(out var entity))
                return 0;

            if (!entity.Has<EffectComponent<TEffect>>())
                return 0;

            return entity.Read<EffectComponent<TEffect>>().Stacks;
        }

        public static bool IsPending<TWorld, TEffect>(EntityGID target)
            where TWorld : struct, IWorldType
            where TEffect : struct, IEffectType
        {
            return target.TryUnpack<TWorld>(out var entity) &&
                   entity.Has<PendingEffectComponent<TEffect>>();
        }

        public static bool Apply<TWorld, TEffect>(
            EntityGID target,
            EntityGID source,
            float duration,
            float period = 0f,
            float delay = 0f
        )
            where TWorld : struct, IWorldType
            where TEffect : struct, IEffectType
        {
            var applyParams = new EffectApplyParams
            {
                Duration = duration,
                Period = period,
                Delay = delay,
            };
            var result = Apply<TWorld, TEffect>(target, source, in applyParams);
            return result == EffectApplyResult.Applied || result == EffectApplyResult.Refreshed;
        }

        /// <summary>
        /// Applies the effect with per-application overrides of its defaults. The handler may refuse the
        /// application first (<see cref="EffectApplyResult.Rejected"/>). A new effect resolves conflicts: a conflicting
        /// effect of the same group with a higher priority rejects it, a lower or equal one is removed. Reapplying the
        /// same effect accumulates or refreshes it as configured. Every application ends in one place that writes the
        /// effect and calls <c>OnApplied</c>; an <see cref="EffectDurationMode.Instant"/> effect has no lifetime, so it is
        /// removed as expired right after that.
        /// </summary>
        public static EffectApplyResult Apply<TWorld, TEffect>(
            EntityGID target,
            EntityGID source,
            in EffectApplyParams applyParams
        )
            where TWorld : struct, IWorldType
            where TEffect : struct, IEffectType
        {
            if (!target.TryUnpack<TWorld>(out var entity))
                return EffectApplyResult.Failed;

            AssertWired<TWorld, TEffect>();

            var config = World<TWorld>.GetResource<EffectConfig<TWorld, TEffect>>();
            if (!TryResolve(config, in applyParams, out var resolved))
                return EffectApplyResult.Failed;

            var current = ReadCurrent<TWorld, TEffect>(entity);
            var currentStacks = current.State.Stacks;
            var checkContext = new EffectApplyContext(
                target,
                source,
                currentStacks,
                currentStacks,
                resolved.Magnitude,
                resolved.SourceName);
            ref var handler = ref World<TWorld>.GetResource<IEffectHandler<TWorld, TEffect>>();
            if (!handler.CanApply(in checkContext))
                return EffectApplyResult.Rejected;

            if (current.Phase == EffectPhase.None &&
                !TryResolveConflicts<TWorld, TEffect>(entity, target, config))
                return EffectApplyResult.ConflictRejected;

            Activate<TWorld, TEffect>(entity, target, source, config, in current, in resolved);
            return current.Phase == EffectPhase.None ? EffectApplyResult.Applied : EffectApplyResult.Refreshed;
        }

        private readonly struct ResolvedApply
        {
            public readonly float Duration;
            public readonly int Charges;
            public readonly float Period;
            public readonly float Delay;
            public readonly float Magnitude;
            public readonly string SourceName;

            public ResolvedApply(
                float duration,
                int charges,
                float period,
                float delay,
                float magnitude,
                string sourceName)
            {
                Duration = duration;
                Charges = charges;
                Period = period;
                Delay = delay;
                Magnitude = magnitude;
                SourceName = sourceName;
            }
        }

        private static bool TryResolve<TWorld, TEffect>(
            EffectConfig<TWorld, TEffect> config,
            in EffectApplyParams applyParams,
            out ResolvedApply resolved)
            where TWorld : struct, IWorldType
            where TEffect : struct, IEffectType
        {
            resolved = default;
            if (config.DurationMode == EffectDurationMode.Instant)
            {
                resolved = new ResolvedApply(0f, 0, 0f, 0f, applyParams.Magnitude, applyParams.SourceName);
                return true;
            }

            var duration = 0f;
            var charges = 0;
            if (config.DurationMode == EffectDurationMode.Time)
            {
                duration = applyParams.Duration > 0f ? applyParams.Duration : config.DefaultDuration;
                if (config.MaxDuration > 0f && duration > config.MaxDuration)
                    duration = config.MaxDuration;

                if (duration <= 0f)
                    return false;
            }
            else
            {
                charges = applyParams.Charges > 0 ? applyParams.Charges : config.DefaultCharges;
                if (config.MaxCharges > 0 && charges > config.MaxCharges)
                    charges = config.MaxCharges;

                if (charges <= 0)
                    return false;
            }

            var period = 0f;
            if (config.ActivationMode == EffectActivationMode.Tick)
            {
                period = config.DefaultPeriod > 0f
                    ? config.DefaultPeriod
                    : StatusEffectSettings.TickSeconds;
            }
            else if (config.ActivationMode == EffectActivationMode.CustomInterval)
            {
                period = applyParams.Period > 0f ? applyParams.Period : config.DefaultPeriod;
                if (period < 0f)
                    period = 0f;
            }

            // A charge effect spends one charge per activation; without a period it would never end.
            if (config.DurationMode == EffectDurationMode.Charges &&
                config.ActivationMode != EffectActivationMode.Condition &&
                period <= 0f)
                return false;

            resolved = new ResolvedApply(
                duration,
                charges,
                period,
                applyParams.Delay > 0f ? applyParams.Delay : 0f,
                applyParams.Magnitude,
                applyParams.SourceName);
            return true;
        }

        private static bool TryResolveConflicts<TWorld, TEffect>(
            World<TWorld>.Entity entity,
            EntityGID target,
            EffectConfig<TWorld, TEffect> config)
            where TWorld : struct, IWorldType
            where TEffect : struct, IEffectType
        {
            if (string.IsNullOrEmpty(config.ConflictGroup) ||
                !World<TWorld>.HasResource<EffectConflictRegistry>() ||
                !World<TWorld>.HasResource<EffectRegistry>() ||
                !entity.Has<World<TWorld>.Multi<EffectSummaryComponent>>())
                return true;

            ref var conflicts = ref World<TWorld>.GetResource<EffectConflictRegistry>();
            ref var registry = ref World<TWorld>.GetResource<EffectRegistry>();
            ref var roster = ref entity.Ref<World<TWorld>.Multi<EffectSummaryComponent>>();

            // Snapshot ids before removing, since removal mutates the roster (RemoveAtSwap).
            System.Span<EffectId> snapshot = stackalloc EffectId[64];
            var count = roster.Length < snapshot.Length ? roster.Length : snapshot.Length;
            for (var i = 0; i < count; i++)
            {
                snapshot[i] = roster[i].Id;
            }

            for (var i = 0; i < count; i++)
            {
                if (conflicts.TryGet(snapshot[i], out var other) &&
                    other.Group == config.ConflictGroup &&
                    other.Priority > config.ConflictPriority)
                    return false;
            }

            for (var i = 0; i < count; i++)
            {
                if (conflicts.TryGet(snapshot[i], out var other) &&
                    other.Group == config.ConflictGroup)
                    registry.InvokeRemove(other.Flag, target);
            }

            return true;
        }

        private enum EffectPhase
        {
            None = 0,
            Pending = 1,
            Active = 2,
        }

        /// <summary>What the target carries of the effect right now: nothing, a delayed effect or a running one.</summary>
        private readonly struct CurrentEffect
        {
            public readonly EffectPhase Phase;
            public readonly EffectLifetimeState State;
            public readonly EntityGID Source;
            public readonly float Magnitude;

            public CurrentEffect(
                EffectPhase phase,
                in EffectLifetimeState state,
                EntityGID source,
                float magnitude)
            {
                Phase = phase;
                State = state;
                Source = source;
                Magnitude = magnitude;
            }
        }

        private static CurrentEffect ReadCurrent<TWorld, TEffect>(World<TWorld>.Entity entity)
            where TWorld : struct, IWorldType
            where TEffect : struct, IEffectType
        {
            if (entity.Has<PendingEffectComponent<TEffect>>())
            {
                var pending = entity.Read<PendingEffectComponent<TEffect>>();
                var state = new EffectLifetimeState
                {
                    Stacks = pending.Stacks,
                    TimeLeft = pending.Duration,
                    ChargesLeft = pending.Charges,
                    Period = pending.Period,
                    PeriodLeft = pending.Period > 0f ? pending.Period : 0f,
                    DelayLeft = pending.DelayLeft,
                };
                return new CurrentEffect(EffectPhase.Pending, in state, pending.Source, pending.Magnitude);
            }

            if (entity.Has<EffectComponent<TEffect>>())
            {
                var active = entity.Read<EffectComponent<TEffect>>();
                var state = new EffectLifetimeState
                {
                    Stacks = active.Stacks,
                    TimeLeft = active.TimeLeft,
                    ChargesLeft = active.ChargesLeft,
                    Period = active.Period,
                    PeriodLeft = active.PeriodLeft,
                };
                return new CurrentEffect(EffectPhase.Active, in state, active.Source, active.Magnitude);
            }

            return default;
        }

        /// <summary>
        /// The one place an application takes effect: merges it into what the target carries (or starts a new effect),
        /// then either schedules it behind its delay or commits it as a running effect.
        /// </summary>
        private static void Activate<TWorld, TEffect>(
            World<TWorld>.Entity entity,
            EntityGID target,
            EntityGID source,
            EffectConfig<TWorld, TEffect> config,
            in CurrentEffect current,
            in ResolvedApply resolved)
            where TWorld : struct, IWorldType
            where TEffect : struct, IEffectType
        {
            var hasCurrent = current.Phase != EffectPhase.None;

            // A delayed effect has no running lifetime to extend, so it never accumulates.
            var rules = EffectMergeRules.From(config, config.Accumulate && current.Phase == EffectPhase.Active);
            var applied = new EffectLifetimeInput(resolved.Duration, resolved.Charges, resolved.Period, resolved.Delay);
            var previous = current.State;
            var next = EffectLifetime.Merge(in previous, in applied, in rules);
            var magnitude = hasCurrent ? current.Magnitude : resolved.Magnitude;

            TrackSource<TWorld, TEffect>(config, hasCurrent, current.Source, source, target);

            if (next.DelayLeft > 0f)
            {
                entity.Set(
                    new PendingEffectComponent<TEffect>
                    {
                        Source = source,
                        DelayLeft = next.DelayLeft,
                        Duration = next.TimeLeft,
                        Period = next.Period,
                        Stacks = next.Stacks,
                        Charges = next.ChargesLeft,
                        Magnitude = magnitude,
                        SourceName = resolved.SourceName,
                    });
                return;
            }

            if (current.Phase == EffectPhase.Pending)
                entity.Delete<PendingEffectComponent<TEffect>>();

            var previousStacks = current.Phase == EffectPhase.Active ? current.State.Stacks : 0;
            Commit<TWorld, TEffect>(
                entity,
                target,
                source,
                in next,
                magnitude,
                resolved.SourceName,
                previousStacks);
        }

        /// <summary>
        /// Tells the handler and the world about the application, then ends the effect at once when nothing is left of it
        /// (an instant effect lives only for its application). A new effect that is already spent is never written to the
        /// target: a component and a roster entry that exist for one call cost more than the application itself.
        /// </summary>
        private static void Commit<TWorld, TEffect>(
            World<TWorld>.Entity entity,
            EntityGID target,
            EntityGID source,
            in EffectLifetimeState next,
            float magnitude,
            string sourceName,
            int previousStacks)
            where TWorld : struct, IWorldType
            where TEffect : struct, IEffectType
        {
            var config = World<TWorld>.GetResource<EffectConfig<TWorld, TEffect>>();
            var exhausted = EffectLifetime.IsExhausted(config.DurationMode, next.TimeLeft, next.ChargesLeft);
            var storesState = !exhausted || previousStacks > 0;
            if (storesState)
            {
                entity.Set(
                    new EffectComponent<TEffect>
                    {
                        Source = source,
                        TimeLeft = next.TimeLeft,
                        Period = next.Period,
                        PeriodLeft = next.PeriodLeft,
                        Stacks = next.Stacks,
                        ChargesLeft = next.ChargesLeft,
                        Magnitude = magnitude,
                        SourceName = sourceName,
                    });
                if (config.Persists)
                    UpdateRosterEntry<TWorld, TEffect>(entity, next.Stacks, next.TimeLeft, next.ChargesLeft);
            }

            var context = new EffectApplyContext(
                target,
                source,
                next.Stacks,
                previousStacks,
                magnitude,
                sourceName);
            ref var handler = ref World<TWorld>.GetResource<IEffectHandler<TWorld, TEffect>>();
            handler.OnApplied(in context);

            World<TWorld>.SendEvent(
                new EffectAppliedEvent<TEffect>
                {
                    Source = source,
                    Target = target,
                    Stacks = next.Stacks,
                    PreviousStacks = previousStacks,
                    TimeLeft = next.TimeLeft,
                });

            if (!exhausted)
                return;

            if (!storesState)
            {
                NotifyRemoved<TWorld, TEffect>(target, source, next.Stacks, expired: true);
                return;
            }

            // A handler may have destroyed the target or removed the effect itself.
            if (target.TryUnpack<TWorld>(out var alive) &&
                alive.Has<EffectComponent<TEffect>>())
                FinalizeEffect<TWorld, TEffect>(alive, target, expired: true);
        }

        private static void NotifyRemoved<TWorld, TEffect>(
            EntityGID target,
            EntityGID source,
            int stacks,
            bool expired)
            where TWorld : struct, IWorldType
            where TEffect : struct, IEffectType
        {
            ref var handler = ref World<TWorld>.GetResource<IEffectHandler<TWorld, TEffect>>();
            handler.OnRemoved(target, source, stacks, expired);

            World<TWorld>.SendEvent(
                new EffectRemovedEvent<TEffect>
                {
                    Source = source,
                    Target = target,
                    Stacks = stacks,
                    Expired = expired,
                }
            );
        }

        internal static void ActivatePending<TWorld, TEffect>(
            World<TWorld>.Entity entity,
            EntityGID target)
            where TWorld : struct, IWorldType
            where TEffect : struct, IEffectType
        {
            if (!entity.Has<PendingEffectComponent<TEffect>>())
                return;

            var pending = entity.Read<PendingEffectComponent<TEffect>>();
            EntityGID source = pending.Source;
            entity.Delete<PendingEffectComponent<TEffect>>();
            var next = new EffectLifetimeState
            {
                Stacks = pending.Stacks,
                TimeLeft = pending.Duration,
                ChargesLeft = pending.Charges,
                Period = pending.Period,
                PeriodLeft = pending.Period > 0f ? pending.Period : 0f,
            };
            Commit<TWorld, TEffect>(
                entity,
                target,
                source,
                in next,
                pending.Magnitude,
                pending.SourceName,
                0);
        }

        private static void TrackSource<TWorld, TEffect>(
            EffectConfig<TWorld, TEffect> config,
            bool hasPrevious,
            EntityGID previousSource,
            EntityGID source,
            EntityGID target)
            where TWorld : struct, IWorldType
            where TEffect : struct, IEffectType
        {
            if (!config.RemoveWithSource)
                return;

            if (hasPrevious && !previousSource.Equals(source))
            {
                EffectBackRefRegistrar.Unregister<TWorld>(
                    previousSource,
                    target,
                    EffectFlagOf<TEffect>.Value);
            }

            EffectBackRefRegistrar.Register<TWorld>(
                source,
                target,
                EffectFlagOf<TEffect>.Value);
        }

        public static bool Remove<TWorld, TEffect>(EntityGID target)
            where TWorld : struct, IWorldType
            where TEffect : struct, IEffectType
        {
            if (!target.TryUnpack<TWorld>(out var entity))
                return false;

            if (entity.Has<PendingEffectComponent<TEffect>>())
            {
                FinalizePending<TWorld, TEffect>(entity, target);
                return true;
            }

            if (!entity.Has<EffectComponent<TEffect>>())
                return false;

            FinalizeEffect<TWorld, TEffect>(entity, target, expired: false);
            return true;
        }

        /// <summary>
        /// Spends one charge of a <see cref="EffectDurationMode.Charges"/> effect on demand, for effects whose charge
        /// is used by the game rather than by the tick system. The effect ends as expired when its last charge is spent.
        /// </summary>
        /// <returns>False when the target has no such effect, the effect is not charge-based or no charge is left.</returns>
        public static bool TrySpendCharge<TWorld, TEffect>(EntityGID target)
            where TWorld : struct, IWorldType
            where TEffect : struct, IEffectType
        {
            if (!target.TryUnpack<TWorld>(out var entity) ||
                !entity.Has<EffectComponent<TEffect>>())
                return false;

            var config = World<TWorld>.GetResource<EffectConfig<TWorld, TEffect>>();
            if (config.DurationMode != EffectDurationMode.Charges)
                return false;

            ref var data = ref entity.Mut<EffectComponent<TEffect>>();
            if (data.ChargesLeft <= 0)
                return false;

            data.ChargesLeft--;
            var chargesLeft = data.ChargesLeft;
            var stacks = data.Stacks;
            var timeLeft = data.TimeLeft;
            UpdateRosterEntry<TWorld, TEffect>(entity, stacks, timeLeft, chargesLeft);

            if (EffectLifetime.IsExhausted(config.DurationMode, timeLeft, chargesLeft))
                FinalizeEffect<TWorld, TEffect>(entity, target, expired: true);

            return true;
        }

        /// <summary>
        /// Removes the effect only if its current <see cref="EffectComponent{TEffect}.Source"/>
        /// matches <paramref name="source"/>. Used by <see cref="EffectRegistry"/> on
        /// source-destroy so an effect re-applied by a different source survives.
        /// </summary>
        internal static bool RemoveFromSource<TWorld, TEffect>(EntityGID target, EntityGID source)
            where TWorld : struct, IWorldType
            where TEffect : struct, IEffectType
        {
            if (!target.TryUnpack<TWorld>(out var entity))
                return false;

            if (entity.Has<PendingEffectComponent<TEffect>>())
            {
                ref var pending = ref entity.Ref<PendingEffectComponent<TEffect>>();
                if (!pending.Source.Equals(source))
                    return false;

                FinalizePending<TWorld, TEffect>(entity, target);
                return true;
            }

            if (!entity.Has<EffectComponent<TEffect>>())
                return false;

            ref var data = ref entity.Ref<EffectComponent<TEffect>>();
            if (!data.Source.Equals(source))
                return false;

            FinalizeEffect<TWorld, TEffect>(entity, target, expired: false);
            return true;
        }

        public static int RemoveAll<TWorld>(EntityGID target)
            where TWorld : struct, IWorldType
        {
            return RemoveByMask<TWorld>(target, (EffectFlag)ulong.MaxValue);
        }

        public static int RemoveByMask<TWorld>(EntityGID target, EffectFlag mask)
            where TWorld : struct, IWorldType
        {
            if (mask == EffectFlag.None)
                return 0;

            if (!target.TryUnpack<TWorld>(out var entity))
                return 0;

            if (!World<TWorld>.HasResource<EffectRegistry>())
                return 0;

            if (!entity.Has<World<TWorld>.Multi<EffectSummaryComponent>>())
                return 0;

            if (!World<TWorld>.HasResource<EffectIdRegistry>())
                return 0;

            ref var idRegistry = ref World<TWorld>.GetResource<EffectIdRegistry>();
            ref var registry = ref World<TWorld>.GetResource<EffectRegistry>();
            ref var roster = ref entity.Ref<World<TWorld>.Multi<EffectSummaryComponent>>();

            var removed = 0;
            // Snapshot ids before invoking callbacks, since callbacks mutate the roster (RemoveAtSwap).
            // For a small N (<= 64), a stack-allocated buffer is enough.
            System.Span<EffectId> snapshot = stackalloc EffectId[64];
            var count = roster.Length < snapshot.Length ? roster.Length : snapshot.Length;
            for (var i = 0; i < count; i++)
            {
                snapshot[i] = roster[i].Id;
            }

            var effectiveMask = (ulong)mask & registry.RegisteredMask;
            if (effectiveMask == 0UL)
                return 0;

            for (var i = 0; i < count; i++)
            {
                if (!idRegistry.TryGetType(snapshot[i], out var type))
                    continue;

                var typeFlag = ResolveFlagFor(type);
                if (((ulong)typeFlag & effectiveMask) == 0UL)
                    continue;

                registry.InvokeRemove(typeFlag, target);
                removed++;
            }

            return removed;
        }

        private static readonly System.Collections.Generic.Dictionary<System.Type, EffectFlag> FlagsByType = new();

        private static EffectFlag ResolveFlagFor(System.Type effectType)
        {
            if (FlagsByType.TryGetValue(effectType, out var cached))
                return cached;

            // Read the [EffectFlag] attribute once per type: the reflection call allocates.
            var attrs = effectType.GetCustomAttributes(typeof(EffectFlagAttribute), inherit: false);
            var flag = attrs.Length == 0 ? EffectFlag.None : ((EffectFlagAttribute)attrs[0]).Flag;
            FlagsByType[effectType] = flag;
            return flag;
        }

        private static void FinalizeEffect<TWorld, TEffect>(
            World<TWorld>.Entity entity,
            EntityGID target,
            bool expired
        )
            where TWorld : struct, IWorldType
            where TEffect : struct, IEffectType
        {
            var snapshot = entity.Read<EffectComponent<TEffect>>();
            EntityGID sourceFull = snapshot.Source;

            entity.Delete<EffectComponent<TEffect>>();

            var config = World<TWorld>.GetResource<EffectConfig<TWorld, TEffect>>();
            if (config.Persists)
                RemoveRosterEntry<TWorld, TEffect>(entity);

            if (config.RemoveWithSource)
                EffectBackRefRegistrar.Unregister<TWorld>(
                    sourceFull,
                    target,
                    EffectFlagOf<TEffect>.Value
                );

            NotifyRemoved<TWorld, TEffect>(target, sourceFull, snapshot.Stacks, expired);
        }

        private static void FinalizePending<TWorld, TEffect>(
            World<TWorld>.Entity entity,
            EntityGID target)
            where TWorld : struct, IWorldType
            where TEffect : struct, IEffectType
        {
            var snapshot = entity.Read<PendingEffectComponent<TEffect>>();
            EntityGID source = snapshot.Source;
            entity.Delete<PendingEffectComponent<TEffect>>();
            if (World<TWorld>.GetResource<EffectConfig<TWorld, TEffect>>().RemoveWithSource)
                EffectBackRefRegistrar.Unregister<TWorld>(
                    source,
                    target,
                    EffectFlagOf<TEffect>.Value);
        }

        /// <summary>Tick-system entry point: completes the same finalize pipeline as
        /// <see cref="Remove{TWorld,TEffect}"/> but flags <see cref="EffectRemovedEvent{TEffect}.Expired"/>
        /// = true. Marked internal so only the framework invokes it.</summary>
        internal static void Expire<TWorld, TEffect>(World<TWorld>.Entity entity, EntityGID target)
            where TWorld : struct, IWorldType
            where TEffect : struct, IEffectType
        {
            FinalizeEffect<TWorld, TEffect>(entity, target, expired: true);
        }

        // --- roster helpers (operate on already-resolved entity, no extra TryUnpack) ---

        private static void AddRosterEntry<TWorld, TEffect>(
            World<TWorld>.Entity entity,
            int stacks,
            float timeLeft,
            int chargesLeft
        )
            where TWorld : struct, IWorldType
            where TEffect : struct, IEffectType
        {
            ref var idRegistry = ref World<TWorld>.GetResource<EffectIdRegistry>();
            if (!idRegistry.TryGet<TEffect>(out var id))
                return;

            entity.Add<World<TWorld>.Multi<EffectSummaryComponent>>();

            ref var roster = ref entity.Ref<World<TWorld>.Multi<EffectSummaryComponent>>();
            roster.Add(
                new EffectSummaryComponent
                {
                    Id = id,
                    Stacks = stacks,
                    TimeLeft = timeLeft,
                    ChargesLeft = chargesLeft,
                }
            );
        }

        private static void UpdateRosterEntry<TWorld, TEffect>(
            World<TWorld>.Entity entity,
            int stacks,
            float timeLeft,
            int chargesLeft
        )
            where TWorld : struct, IWorldType
            where TEffect : struct, IEffectType
        {
            ref var idRegistry = ref World<TWorld>.GetResource<EffectIdRegistry>();
            if (!idRegistry.TryGet<TEffect>(out var id))
                return;

            if (!entity.Has<World<TWorld>.Multi<EffectSummaryComponent>>())
            {
                AddRosterEntry<TWorld, TEffect>(entity, stacks, timeLeft, chargesLeft);
                return;
            }

            ref var roster = ref entity.Ref<World<TWorld>.Multi<EffectSummaryComponent>>();
            for (var i = 0; i < roster.Length; i++)
            {
                if (roster[i].Id.Equals(id))
                {
                    roster[i].Stacks = stacks;
                    roster[i].TimeLeft = timeLeft;
                    roster[i].ChargesLeft = chargesLeft;
                    return;
                }
            }

            roster.Add(
                new EffectSummaryComponent
                {
                    Id = id,
                    Stacks = stacks,
                    TimeLeft = timeLeft,
                    ChargesLeft = chargesLeft,
                }
            );
        }

        private static void RemoveRosterEntry<TWorld, TEffect>(World<TWorld>.Entity entity)
            where TWorld : struct, IWorldType
            where TEffect : struct, IEffectType
        {
            if (!World<TWorld>.HasResource<EffectIdRegistry>())
                return;

            ref var idRegistry = ref World<TWorld>.GetResource<EffectIdRegistry>();
            if (!idRegistry.TryGet<TEffect>(out var id))
                return;

            if (!entity.Has<World<TWorld>.Multi<EffectSummaryComponent>>())
                return;

            ref var roster = ref entity.Ref<World<TWorld>.Multi<EffectSummaryComponent>>();
            for (var i = 0; i < roster.Length; i++)
            {
                if (roster[i].Id.Equals(id))
                {
                    roster.RemoveAtSwap(i);
                    return;
                }
            }
        }

        [Conditional("DEBUG")]
        private static void AssertWired<TWorld, TEffect>()
            where TWorld : struct, IWorldType
            where TEffect : struct, IEffectType
        {
            if (!World<TWorld>.HasResource<EffectIdRegistry>())
                throw new System.InvalidOperationException(
                    $"EffectIdRegistry is not registered for world {typeof(TWorld).Name}. "
                        + $"Did you forget to add EffectFeature<{typeof(TWorld).Name}, {typeof(TEffect).Name}>?"
                );

            if (!World<TWorld>.HasResource<IEffectHandler<TWorld, TEffect>>())
                throw new System.InvalidOperationException(
                    $"IEffectHandler<{typeof(TWorld).Name}, {typeof(TEffect).Name}> is not registered. "
                        + $"Did you forget to add EffectFeature<{typeof(TWorld).Name}, {typeof(TEffect).Name}>?"
                );

            if (!World<TWorld>.HasResource<EffectConfig<TWorld, TEffect>>())
                throw new System.InvalidOperationException(
                    $"EffectConfig<{typeof(TWorld).Name}, {typeof(TEffect).Name}> is not registered."
                );
        }

        // --- Main-default overloads ---

        public static bool Has<TEffect>(EntityGID target)
            where TEffect : struct, IEffectType => Has<Main, TEffect>(target);

        public static float GetTimeLeft<TEffect>(EntityGID target)
            where TEffect : struct, IEffectType => GetTimeLeft<Main, TEffect>(target);

        public static int GetStacks<TEffect>(EntityGID target)
            where TEffect : struct, IEffectType => GetStacks<Main, TEffect>(target);

        public static bool IsPending<TEffect>(EntityGID target)
            where TEffect : struct, IEffectType => IsPending<Main, TEffect>(target);

        public static bool Apply<TEffect>(
            EntityGID target,
            EntityGID source,
            float duration,
            float period = 0f,
            float delay = 0f
        )
            where TEffect : struct, IEffectType =>
            Apply<Main, TEffect>(target, source, duration, period, delay);

        public static bool Remove<TEffect>(EntityGID target)
            where TEffect : struct, IEffectType => Remove<Main, TEffect>(target);

        public static int RemoveAll(EntityGID target) => RemoveAll<Main>(target);

        public static int RemoveByMask(EntityGID target, EffectFlag mask) =>
            RemoveByMask<Main>(target, mask);
    }
}
