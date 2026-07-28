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
            if (duration <= 0f)
                return false;

            if (!target.TryUnpack<TWorld>(out var entity))
                return false;

            AssertWired<TWorld, TEffect>();

            if (period < 0f)
                period = 0f;

            if (delay < 0f)
                delay = 0f;

            var compactSource = (EntityGIDCompact)source;
            if (entity.Has<PendingEffectComponent<TEffect>>())
                return ReapplyPending<TWorld, TEffect>(
                    entity,
                    target,
                    source,
                    compactSource,
                    duration,
                    period,
                    delay);

            if (entity.Has<EffectComponent<TEffect>>())
                return ReapplyActive<TWorld, TEffect>(
                    entity,
                    target,
                    source,
                    compactSource,
                    duration,
                    period);

            EffectBackRefRegistrar.Register<TWorld>(
                source,
                target,
                EffectFlagOf<TEffect>.Value);

            if (delay > 0f)
            {
                entity.Set(
                    new PendingEffectComponent<TEffect>
                    {
                        Source = compactSource,
                        DelayLeft = delay,
                        Duration = duration,
                        Period = period,
                        Stacks = 1,
                    });
                return true;
            }

            Activate<TWorld, TEffect>(
                entity,
                target,
                source,
                compactSource,
                duration,
                period,
                1);
            return true;
        }

        private static bool ReapplyPending<TWorld, TEffect>(
            World<TWorld>.Entity entity,
            EntityGID target,
            EntityGID source,
            EntityGIDCompact compactSource,
            float duration,
            float period,
            float delay)
            where TWorld : struct, IWorldType
            where TEffect : struct, IEffectType
        {
            ref var data = ref entity.Ref<PendingEffectComponent<TEffect>>();
            ref var config = ref World<TWorld>.GetResource<EffectConfig<TWorld, TEffect>>();
            var previousSource = data.Source;

            data.Stacks++;
            if (data.Stacks > config.MaxStacks)
                data.Stacks = config.MaxStacks;

            data.Source = compactSource;
            if (config.RefreshOnReapply)
            {
                if (duration > data.Duration)
                    data.Duration = duration;

                data.DelayLeft = delay;
                data.Period = period;
            }

            UpdateBackReference<TWorld, TEffect>(
                previousSource,
                compactSource,
                source,
                target);

            if (data.DelayLeft > 0f)
                return true;

            var snapshot = data;
            entity.Delete<PendingEffectComponent<TEffect>>();
            Activate<TWorld, TEffect>(
                entity,
                target,
                source,
                snapshot.Source,
                snapshot.Duration,
                snapshot.Period,
                snapshot.Stacks);
            return true;
        }

        private static bool ReapplyActive<TWorld, TEffect>(
            World<TWorld>.Entity entity,
            EntityGID target,
            EntityGID source,
            EntityGIDCompact compactSource,
            float duration,
            float period)
            where TWorld : struct, IWorldType
            where TEffect : struct, IEffectType
        {
            ref var data = ref entity.Ref<EffectComponent<TEffect>>();
            ref var config = ref World<TWorld>.GetResource<EffectConfig<TWorld, TEffect>>();
            var previousSource = data.Source;
            var previousStacks = data.Stacks;
            var newStacks = previousStacks + 1;
            if (newStacks > config.MaxStacks)
                newStacks = config.MaxStacks;

            data.Stacks = newStacks;
            data.Source = compactSource;
            if (config.RefreshOnReapply)
            {
                if (duration > data.TimeLeft)
                    data.TimeLeft = duration;

                data.Period = period;
                data.PeriodLeft = period > 0f ? period : 0f;
            }

            UpdateBackReference<TWorld, TEffect>(
                previousSource,
                compactSource,
                source,
                target);
            UpdateRosterEntry<TWorld, TEffect>(entity, newStacks, data.TimeLeft);

            ref var handler = ref World<TWorld>.GetResource<IEffectHandler<TWorld, TEffect>>();
            handler.OnApplied(target, source, newStacks, previousStacks);

            World<TWorld>.SendEvent(
                new EffectRefreshedEvent<TEffect>
                {
                    Source = source,
                    Target = target,
                    Stacks = newStacks,
                    PreviousStacks = previousStacks,
                    TimeLeft = data.TimeLeft,
                });

            return true;
        }

        private static void Activate<TWorld, TEffect>(
            World<TWorld>.Entity entity,
            EntityGID target,
            EntityGID source,
            EntityGIDCompact compactSource,
            float duration,
            float period,
            int stacks)
            where TWorld : struct, IWorldType
            where TEffect : struct, IEffectType
        {
            entity.Set(
                new EffectComponent<TEffect>
                {
                    Source = compactSource,
                    TimeLeft = duration,
                    Period = period,
                    PeriodLeft = period > 0f ? period : 0f,
                    Stacks = stacks,
                });
            AddRosterEntry<TWorld, TEffect>(entity, stacks, duration);

            ref var handler = ref World<TWorld>.GetResource<IEffectHandler<TWorld, TEffect>>();
            handler.OnApplied(target, source, stacks, 0);

            World<TWorld>.SendEvent(
                new EffectAppliedEvent<TEffect>
                {
                    Source = source,
                    Target = target,
                    Stacks = stacks,
                    TimeLeft = duration,
                });
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
            Activate<TWorld, TEffect>(
                entity,
                target,
                source,
                pending.Source,
                pending.Duration,
                pending.Period,
                pending.Stacks);
        }

        private static void UpdateBackReference<TWorld, TEffect>(
            EntityGIDCompact previousSource,
            EntityGIDCompact currentSource,
            EntityGID source,
            EntityGID target)
            where TWorld : struct, IWorldType
            where TEffect : struct, IEffectType
        {
            if (!previousSource.Equals(currentSource))
            {
                EntityGID oldSource = previousSource;
                EffectBackRefRegistrar.Unregister<TWorld>(
                    oldSource,
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

            var compactSource = (EntityGIDCompact)source;
            if (entity.Has<PendingEffectComponent<TEffect>>())
            {
                ref var pending = ref entity.Ref<PendingEffectComponent<TEffect>>();
                if (!pending.Source.Equals(compactSource))
                    return false;

                FinalizePending<TWorld, TEffect>(entity, target);
                return true;
            }

            if (!entity.Has<EffectComponent<TEffect>>())
                return false;

            ref var data = ref entity.Ref<EffectComponent<TEffect>>();
            if (!data.Source.Equals(compactSource))
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

        private static EffectFlag ResolveFlagFor(System.Type effectType)
        {
            // Read the [EffectFlag] attribute via the same path EffectFlagOf<T> uses, but on a runtime Type.
            var attrs = effectType.GetCustomAttributes(typeof(EffectFlagAttribute), inherit: false);
            if (attrs.Length == 0)
                return EffectFlag.None;

            return ((EffectFlagAttribute)attrs[0]).Flag;
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
            RemoveRosterEntry<TWorld, TEffect>(entity);

            EffectBackRefRegistrar.Unregister<TWorld>(
                sourceFull,
                target,
                EffectFlagOf<TEffect>.Value
            );

            ref var handler = ref World<TWorld>.GetResource<IEffectHandler<TWorld, TEffect>>();
            handler.OnRemoved(target, sourceFull, snapshot.Stacks, expired);

            World<TWorld>.SendEvent(
                new EffectRemovedEvent<TEffect>
                {
                    Source = sourceFull,
                    Target = target,
                    Stacks = snapshot.Stacks,
                    Expired = expired,
                }
            );
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
            float timeLeft
        )
            where TWorld : struct, IWorldType
            where TEffect : struct, IEffectType
        {
            ref var idRegistry = ref World<TWorld>.GetResource<EffectIdRegistry>();
            if (!idRegistry.TryGet<TEffect>(out var id))
                return;

            if (!entity.Has<World<TWorld>.Multi<EffectSummaryComponent>>())
                entity.Add<World<TWorld>.Multi<EffectSummaryComponent>>();

            ref var roster = ref entity.Ref<World<TWorld>.Multi<EffectSummaryComponent>>();
            roster.Add(
                new EffectSummaryComponent
                {
                    Id = id,
                    Stacks = stacks,
                    TimeLeft = timeLeft,
                }
            );
        }

        private static void UpdateRosterEntry<TWorld, TEffect>(
            World<TWorld>.Entity entity,
            int stacks,
            float timeLeft
        )
            where TWorld : struct, IWorldType
            where TEffect : struct, IEffectType
        {
            ref var idRegistry = ref World<TWorld>.GetResource<EffectIdRegistry>();
            if (!idRegistry.TryGet<TEffect>(out var id))
                return;

            if (!entity.Has<World<TWorld>.Multi<EffectSummaryComponent>>())
            {
                AddRosterEntry<TWorld, TEffect>(entity, stacks, timeLeft);
                return;
            }

            ref var roster = ref entity.Ref<World<TWorld>.Multi<EffectSummaryComponent>>();
            for (var i = 0; i < roster.Length; i++)
            {
                if (roster[i].Id.Equals(id))
                {
                    roster[i].Stacks = stacks;
                    roster[i].TimeLeft = timeLeft;
                    return;
                }
            }

            roster.Add(
                new EffectSummaryComponent
                {
                    Id = id,
                    Stacks = stacks,
                    TimeLeft = timeLeft,
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
