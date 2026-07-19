using System.Diagnostics;
using FFS.Libraries.StaticEcs;


namespace UniGame.StaticEcs.Features
{
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
            {
                return false;
            }

            return entity.Has<EffectComponent<TEffect>>();
        }

        public static float GetTimeLeft<TWorld, TEffect>(EntityGID target)
            where TWorld : struct, IWorldType
            where TEffect : struct, IEffectType
        {
            if (!target.TryUnpack<TWorld>(out var entity))
            {
                return 0f;
            }

            if (!entity.Has<EffectComponent<TEffect>>())
            {
                return 0f;
            }

            return entity.Read<EffectComponent<TEffect>>().TimeLeft;
        }

        public static int GetStacks<TWorld, TEffect>(EntityGID target)
            where TWorld : struct, IWorldType
            where TEffect : struct, IEffectType
        {
            if (!target.TryUnpack<TWorld>(out var entity))
            {
                return 0;
            }

            if (!entity.Has<EffectComponent<TEffect>>())
            {
                return 0;
            }

            return entity.Read<EffectComponent<TEffect>>().Stacks;
        }

        public static bool Apply<TWorld, TEffect>(
            EntityGID target,
            EntityGID source,
            float duration,
            float period = 0f,
            float delay = 0f)
            where TWorld : struct, IWorldType
            where TEffect : struct, IEffectType
        {
            if (duration <= 0f)
            {
                return false;
            }

            if (!target.TryUnpack<TWorld>(out var entity))
            {
                return false;
            }

            AssertWired<TWorld, TEffect>();

            if (period < 0f)
            {
                period = 0f;
            }

            if (delay < 0f)
            {
                delay = 0f;
            }

            var initialPeriodLeft = period > 0f ? (delay > 0f ? delay + period : period) : 0f;
            var compactSource = (EntityGIDCompact)source;

            var isFresh = !entity.Has<EffectComponent<TEffect>>();
            if (isFresh)
            {
                entity.Add<EffectComponent<TEffect>>();
            }

            ref var data = ref entity.Ref<EffectComponent<TEffect>>();

            int previousStacks;
            int newStacks;
            float resultingTimeLeft;

            if (isFresh)
            {
                previousStacks = 0;
                newStacks = 1;
                resultingTimeLeft = duration;

                data.Source = compactSource;
                data.DelayLeft = delay;
                data.TimeLeft = duration;
                data.Period = period;
                data.PeriodLeft = initialPeriodLeft;
                data.Stacks = 1;
            }
            else
            {
                ref var config = ref World<TWorld>.GetResource<EffectConfig<TWorld, TEffect>>();
                previousStacks = data.Stacks;
                newStacks = previousStacks + 1;
                if (newStacks > config.MaxStacks)
                {
                    newStacks = config.MaxStacks;
                }

                data.Stacks = newStacks;
                data.Source = compactSource;

                if (config.RefreshOnReapply)
                {
                    if (duration > data.TimeLeft)
                    {
                        data.TimeLeft = duration;
                    }

                    data.Period = period;
                    data.DelayLeft = delay;
                    data.PeriodLeft = initialPeriodLeft;
                }

                resultingTimeLeft = data.TimeLeft;
            }

            EffectBackRefRegistrar.Register<TWorld>(source, target, EffectFlagOf<TEffect>.Value);

            if (isFresh)
            {
                AddRosterEntry<TWorld, TEffect>(entity, newStacks, resultingTimeLeft);
            }
            else
            {
                UpdateRosterEntry<TWorld, TEffect>(entity, newStacks, resultingTimeLeft);
            }

            ref var handler = ref World<TWorld>.GetResource<IEffectHandler<TWorld, TEffect>>();
            handler.OnApplied(target, source, newStacks, previousStacks);

            if (isFresh)
            {
                World<TWorld>.SendEvent(new EffectAppliedEvent<TEffect>
                {
                    Source = source,
                    Target = target,
                    Stacks = newStacks,
                    TimeLeft = resultingTimeLeft
                });
            }
            else
            {
                World<TWorld>.SendEvent(new EffectRefreshedEvent<TEffect>
                {
                    Source = source,
                    Target = target,
                    Stacks = newStacks,
                    PreviousStacks = previousStacks,
                    TimeLeft = resultingTimeLeft
                });
            }

            return true;
        }

        public static bool Remove<TWorld, TEffect>(EntityGID target)
            where TWorld : struct, IWorldType
            where TEffect : struct, IEffectType
        {
            if (!target.TryUnpack<TWorld>(out var entity))
            {
                return false;
            }

            if (!entity.Has<EffectComponent<TEffect>>())
            {
                return false;
            }

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
            {
                return false;
            }

            if (!entity.Has<EffectComponent<TEffect>>())
            {
                return false;
            }

            var compactSource = (EntityGIDCompact)source;
            ref var data = ref entity.Ref<EffectComponent<TEffect>>();
            if (!data.Source.Equals(compactSource))
            {
                return false;
            }

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
            {
                return 0;
            }

            if (!target.TryUnpack<TWorld>(out var entity))
            {
                return 0;
            }

            if (!World<TWorld>.HasResource<EffectRegistry>())
            {
                return 0;
            }

            if (!entity.Has<World<TWorld>.Multi<EffectRosterEntry>>())
            {
                return 0;
            }

            if (!World<TWorld>.HasResource<EffectIdRegistry>())
            {
                return 0;
            }

            ref var idRegistry = ref World<TWorld>.GetResource<EffectIdRegistry>();
            ref var registry = ref World<TWorld>.GetResource<EffectRegistry>();
            ref var roster = ref entity.Ref<World<TWorld>.Multi<EffectRosterEntry>>();

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
            {
                return 0;
            }

            for (var i = 0; i < count; i++)
            {
                if (!idRegistry.TryGetType(snapshot[i], out var type))
                {
                    continue;
                }

                var typeFlag = ResolveFlagFor(type);
                if (((ulong)typeFlag & effectiveMask) == 0UL)
                {
                    continue;
                }

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
            {
                return EffectFlag.None;
            }

            return ((EffectFlagAttribute)attrs[0]).Flag;
        }

        private static void FinalizeEffect<TWorld, TEffect>(
            World<TWorld>.Entity entity,
            EntityGID target,
            bool expired)
            where TWorld : struct, IWorldType
            where TEffect : struct, IEffectType
        {
            var snapshot = entity.Read<EffectComponent<TEffect>>();
            EntityGID sourceFull = snapshot.Source;

            entity.Delete<EffectComponent<TEffect>>();
            RemoveRosterEntry<TWorld, TEffect>(entity);

            EffectBackRefRegistrar.Unregister<TWorld>(sourceFull, target, EffectFlagOf<TEffect>.Value);

            ref var handler = ref World<TWorld>.GetResource<IEffectHandler<TWorld, TEffect>>();
            handler.OnRemoved(target, sourceFull, snapshot.Stacks, expired);

            World<TWorld>.SendEvent(new EffectRemovedEvent<TEffect>
            {
                Source = sourceFull,
                Target = target,
                Stacks = snapshot.Stacks,
                Expired = expired
            });
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
            World<TWorld>.Entity entity, int stacks, float timeLeft)
            where TWorld : struct, IWorldType
            where TEffect : struct, IEffectType
        {
            ref var idRegistry = ref World<TWorld>.GetResource<EffectIdRegistry>();
            if (!idRegistry.TryGet<TEffect>(out var id))
            {
                return;
            }

            if (!entity.Has<World<TWorld>.Multi<EffectRosterEntry>>())
            {
                entity.Add<World<TWorld>.Multi<EffectRosterEntry>>();
            }

            ref var roster = ref entity.Ref<World<TWorld>.Multi<EffectRosterEntry>>();
            roster.Add(new EffectRosterEntry { Id = id, Stacks = stacks, TimeLeft = timeLeft });
        }

        private static void UpdateRosterEntry<TWorld, TEffect>(
            World<TWorld>.Entity entity, int stacks, float timeLeft)
            where TWorld : struct, IWorldType
            where TEffect : struct, IEffectType
        {
            ref var idRegistry = ref World<TWorld>.GetResource<EffectIdRegistry>();
            if (!idRegistry.TryGet<TEffect>(out var id))
            {
                return;
            }

            if (!entity.Has<World<TWorld>.Multi<EffectRosterEntry>>())
            {
                AddRosterEntry<TWorld, TEffect>(entity, stacks, timeLeft);
                return;
            }

            ref var roster = ref entity.Ref<World<TWorld>.Multi<EffectRosterEntry>>();
            for (var i = 0; i < roster.Length; i++)
            {
                if (roster[i].Id.Equals(id))
                {
                    roster[i].Stacks = stacks;
                    roster[i].TimeLeft = timeLeft;
                    return;
                }
            }

            roster.Add(new EffectRosterEntry { Id = id, Stacks = stacks, TimeLeft = timeLeft });
        }

        private static void RemoveRosterEntry<TWorld, TEffect>(World<TWorld>.Entity entity)
            where TWorld : struct, IWorldType
            where TEffect : struct, IEffectType
        {
            if (!World<TWorld>.HasResource<EffectIdRegistry>())
            {
                return;
            }

            ref var idRegistry = ref World<TWorld>.GetResource<EffectIdRegistry>();
            if (!idRegistry.TryGet<TEffect>(out var id))
            {
                return;
            }

            if (!entity.Has<World<TWorld>.Multi<EffectRosterEntry>>())
            {
                return;
            }

            ref var roster = ref entity.Ref<World<TWorld>.Multi<EffectRosterEntry>>();
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
            {
                throw new System.InvalidOperationException(
                    $"EffectIdRegistry is not registered for world {typeof(TWorld).Name}. " +
                    $"Did you forget to add EffectFeature<{typeof(TWorld).Name}, {typeof(TEffect).Name}>?");
            }

            if (!World<TWorld>.HasResource<IEffectHandler<TWorld, TEffect>>())
            {
                throw new System.InvalidOperationException(
                    $"IEffectHandler<{typeof(TWorld).Name}, {typeof(TEffect).Name}> is not registered. " +
                    $"Did you forget to add EffectFeature<{typeof(TWorld).Name}, {typeof(TEffect).Name}>?");
            }

            if (!World<TWorld>.HasResource<EffectConfig<TWorld, TEffect>>())
            {
                throw new System.InvalidOperationException(
                    $"EffectConfig<{typeof(TWorld).Name}, {typeof(TEffect).Name}> is not registered.");
            }
        }

        // --- Main-default overloads ---

        public static bool Has<TEffect>(EntityGID target) where TEffect : struct, IEffectType
            => Has<Main, TEffect>(target);

        public static float GetTimeLeft<TEffect>(EntityGID target) where TEffect : struct, IEffectType
            => GetTimeLeft<Main, TEffect>(target);

        public static int GetStacks<TEffect>(EntityGID target) where TEffect : struct, IEffectType
            => GetStacks<Main, TEffect>(target);

        public static bool Apply<TEffect>(
            EntityGID target,
            EntityGID source,
            float duration,
            float period = 0f,
            float delay = 0f)
            where TEffect : struct, IEffectType
            => Apply<Main, TEffect>(target, source, duration, period, delay);

        public static bool Remove<TEffect>(EntityGID target) where TEffect : struct, IEffectType
            => Remove<Main, TEffect>(target);

        public static int RemoveAll(EntityGID target) => RemoveAll<Main>(target);

        public static int RemoveByMask(EntityGID target, EffectFlag mask)
            => RemoveByMask<Main>(target, mask);
    }
}