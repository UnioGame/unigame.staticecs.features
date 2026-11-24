namespace UniGame.StaticEcs.Features.Tests
{
    using FFS.Libraries.StaticEcs;
    using NUnit.Framework;
    using UniGame.StaticEcs.Tests;
    using UniGame.StaticEcs.Time;

    /// <summary>
    /// Fixes what <c>EffectOperations.Apply</c> does to an effect that is already on the target, and what the handler
    /// sees on each kind of application, so the application path can be restructured without changing behavior.
    /// </summary>
    [TestFixture]
    public sealed class EffectReapplyBehaviorTests
    {
        private RecordingEffectHandler<TestEffectsWorld, TestEffectMarker> _handler;
        private StaticEcsTestWorld<TestEffectsWorld> _world;

        [TearDown]
        public void TearDown()
        {
            _world?.Dispose();
            _world = null;
        }

        [Test]
        public void CanApply_SeesTheStacksCurrentlyOnTheTarget()
        {
            CreateWorld(Config(maxStacks: 3, refreshOnReapply: true));
            var (source, target) = CreatePair();

            Apply(target, source, new EffectApplyParams { Duration = 5f });
            Apply(target, source, new EffectApplyParams { Duration = 5f });

            Assert.AreEqual((0, 0), _handler.Checked[0], "a new effect is checked with no stacks");
            Assert.AreEqual((1, 1), _handler.Checked[1], "a reapply is checked with the stacks already present");
        }

        [Test]
        public void OnApplied_ReceivesStacksAfterAndBeforeEachApplication()
        {
            CreateWorld(Config(maxStacks: 2, refreshOnReapply: true));
            var (source, target) = CreatePair();

            for (var i = 0; i < 3; i++)
                Apply(target, source, new EffectApplyParams { Duration = 5f });

            Assert.AreEqual(3, _handler.Applied.Count);
            Assert.AreEqual((1, 0), (_handler.Applied[0].Stacks, _handler.Applied[0].PreviousStacks));
            Assert.AreEqual((2, 1), (_handler.Applied[1].Stacks, _handler.Applied[1].PreviousStacks));
            Assert.AreEqual((2, 2), (_handler.Applied[2].Stacks, _handler.Applied[2].PreviousStacks), "capped by MaxStacks");
        }

        [Test]
        public void Reapply_RefusedByHandler_ReturnsRejectedAndChangesNothing()
        {
            CreateWorld(Config(maxStacks: 3, refreshOnReapply: true));
            var (source, target) = CreatePair();
            Apply(target, source, new EffectApplyParams { Duration = 3f });
            _handler.RejectApply = true;

            var result = Apply(target, source, new EffectApplyParams { Duration = 5f });

            Assert.AreEqual(EffectApplyResult.Rejected, result);
            var effect = target.Read<EffectComponent<TestEffectMarker>>();
            Assert.AreEqual(1, effect.Stacks);
            Assert.AreEqual(3f, effect.TimeLeft, 0.0001f);
            Assert.AreEqual(1, _handler.Applied.Count);
        }

        [Test]
        public void Reapply_WithRefreshAndAccumulateOff_KeepsTheValuesButStillNotifiesTheHandler()
        {
            CreateWorld(Config(maxStacks: 2, refreshOnReapply: false));
            var (source, target) = CreatePair();
            Apply(target, source, new EffectApplyParams { Duration = 5f });
            target.Ref<EffectComponent<TestEffectMarker>>().TimeLeft = 2f;

            var result = Apply(target, source, new EffectApplyParams { Duration = 5f });

            Assert.AreEqual(EffectApplyResult.Refreshed, result);
            var effect = target.Read<EffectComponent<TestEffectMarker>>();
            Assert.AreEqual(2f, effect.TimeLeft, 0.0001f, "the lifetime is neither refreshed nor accumulated");
            Assert.AreEqual(2, effect.Stacks);
            Assert.AreEqual(2, _handler.Applied.Count);
        }

        [Test]
        public void Refresh_ResetsThePeriodAndItsPhase()
        {
            CreateWorld(Config(maxStacks: 1, refreshOnReapply: true));
            var (source, target) = CreatePair();
            Apply(target, source, new EffectApplyParams { Duration = 5f, Period = 1f });
            target.Ref<EffectComponent<TestEffectMarker>>().PeriodLeft = 0.2f;

            Apply(target, source, new EffectApplyParams { Duration = 5f, Period = 2f });

            var effect = target.Read<EffectComponent<TestEffectMarker>>();
            Assert.AreEqual(2f, effect.Period, 0.0001f);
            Assert.AreEqual(2f, effect.PeriodLeft, 0.0001f);
        }

        [Test]
        public void Refresh_KeepsTheMagnitudeOfTheFirstApplication()
        {
            CreateWorld(Config(maxStacks: 2, refreshOnReapply: true));
            var (source, target) = CreatePair();
            Apply(target, source, new EffectApplyParams { Duration = 5f, Magnitude = 5f });

            Apply(target, source, new EffectApplyParams { Duration = 5f, Magnitude = 9f });

            Assert.AreEqual(5f, target.Read<EffectComponent<TestEffectMarker>>().Magnitude);
            Assert.AreEqual(5f, _handler.Magnitudes[1], "the handler of a reapply sees the first magnitude");
        }

        [Test]
        public void Reapply_UpdatesTheRosterEntry()
        {
            CreateWorld(Config(maxStacks: 3, refreshOnReapply: true));
            var (source, target) = CreatePair();
            Apply(target, source, new EffectApplyParams { Duration = 3f });

            Apply(target, source, new EffectApplyParams { Duration = 5f });

            ref var roster = ref target.Ref<World<TestEffectsWorld>.Multi<EffectSummaryComponent>>();
            Assert.AreEqual(1, roster.Length, "a reapply does not add a second entry");
            Assert.AreEqual(2, roster[0].Stacks);
            Assert.AreEqual(5f, roster[0].TimeLeft, 0.0001f);
        }

        [Test]
        public void Reapply_FromAnotherSource_ReplacesTheSourceAndMovesTheBackReference()
        {
            CreateWorld(Config(maxStacks: 1, refreshOnReapply: true));
            var target = World<TestEffectsWorld>.NewEntity<Default>();
            var first = World<TestEffectsWorld>.NewEntity<Default>();
            var second = World<TestEffectsWorld>.NewEntity<Default>();
            Apply(target, first, new EffectApplyParams { Duration = 3f, SourceName = "First" });

            Apply(target, second, new EffectApplyParams { Duration = 3f, SourceName = "Second" });

            var effect = target.Read<EffectComponent<TestEffectMarker>>();
            Assert.AreEqual(second.GID, (EntityGID)effect.Source);
            Assert.AreEqual("Second", effect.SourceName);
            Assert.AreEqual(0, BackReferenceCount(first), "the previous source no longer tracks the target");
            Assert.AreEqual(1, BackReferenceCount(second));

            first.Destroy();
            Assert.IsTrue(target.Has<EffectComponent<TestEffectMarker>>(), "the old source cannot remove it any more");

            second.Destroy();
            Assert.IsFalse(target.Has<EffectComponent<TestEffectMarker>>(), "the new source removes it");
        }

        [Test]
        public void ReapplyDuringDelay_NeverAccumulatesEvenWhenTheEffectAccumulates()
        {
            var config = Config(maxStacks: 3, refreshOnReapply: true);
            config.Accumulate = true;
            CreateWorld(config);
            var (source, target) = CreatePair();
            Apply(target, source, new EffectApplyParams { Duration = 1f, Delay = 2f });

            var result = Apply(target, source, new EffectApplyParams { Duration = 3f, Delay = 2f });

            Assert.AreEqual(EffectApplyResult.Refreshed, result);
            var pending = target.Read<PendingEffectComponent<TestEffectMarker>>();
            Assert.AreEqual(3f, pending.Duration, 0.0001f, "a delayed effect takes the larger duration, it does not add");
            Assert.AreEqual(2, pending.Stacks);
        }

        [Test]
        public void ReapplyDuringDelay_WithoutADelay_ActivatesTheEffectAtOnce()
        {
            CreateWorld(Config(maxStacks: 3, refreshOnReapply: true));
            var (source, target) = CreatePair();
            Apply(target, source, new EffectApplyParams { Duration = 1f, Delay = 2f });

            var result = Apply(target, source, new EffectApplyParams { Duration = 3f });

            Assert.AreEqual(EffectApplyResult.Refreshed, result);
            Assert.IsFalse(target.Has<PendingEffectComponent<TestEffectMarker>>());
            var effect = target.Read<EffectComponent<TestEffectMarker>>();
            Assert.AreEqual(3f, effect.TimeLeft, 0.0001f);
            Assert.AreEqual(2, effect.Stacks);
            Assert.AreEqual(1, _handler.Applied.Count);
            Assert.AreEqual(
                (2, 0),
                (_handler.Applied[0].Stacks, _handler.Applied[0].PreviousStacks),
                "an effect that leaves its delay is applied as a new one");
        }

        [Test]
        public void Apply_WithADelayOnARunningEffect_DoesNotDelayIt()
        {
            CreateWorld(Config(maxStacks: 3, refreshOnReapply: true));
            var (source, target) = CreatePair();
            Apply(target, source, new EffectApplyParams { Duration = 3f });

            Apply(target, source, new EffectApplyParams { Duration = 5f, Delay = 4f });

            Assert.IsFalse(target.Has<PendingEffectComponent<TestEffectMarker>>());
            Assert.AreEqual(5f, target.Read<EffectComponent<TestEffectMarker>>().TimeLeft, 0.0001f);
        }

        [Test]
        public void Apply_WithALifetimeAlreadySpent_NeverWritesTheEffectButStillAppliesAndRemovesIt()
        {
            CreateWorld(Config(maxStacks: 1, refreshOnReapply: true));
            var (source, target) = CreatePair();

            var result = Apply(target, source, new EffectApplyParams { Duration = EffectLifetime.TimeEpsilon / 2f });

            Assert.AreEqual(EffectApplyResult.Applied, result);
            Assert.IsFalse(target.Has<EffectComponent<TestEffectMarker>>());
            Assert.IsFalse(target.Has<World<TestEffectsWorld>.Multi<EffectSummaryComponent>>());
            Assert.AreEqual(1, _handler.Applied.Count);
            Assert.AreEqual(1, _handler.Removed.Count);
            Assert.IsTrue(_handler.Removed[0].Expired);
        }

        private static int BackReferenceCount(World<TestEffectsWorld>.Entity source)
        {
            if (!source.Has<World<TestEffectsWorld>.Multi<EffectTargetComponent>>())
                return 0;

            return source.Read<World<TestEffectsWorld>.Multi<EffectTargetComponent>>().Length;
        }

        private static EffectApplyResult Apply(
            World<TestEffectsWorld>.Entity target,
            World<TestEffectsWorld>.Entity source,
            EffectApplyParams applyParams)
        {
            return EffectOperations.Apply<TestEffectsWorld, TestEffectMarker>(target.GID, source.GID, in applyParams);
        }

        private static EffectConfig<TestEffectsWorld, TestEffectMarker> Config(int maxStacks, bool refreshOnReapply)
        {
            return new EffectConfig<TestEffectsWorld, TestEffectMarker>(
                maxStacks,
                refreshOnReapply,
                registerTickSystem: false);
        }

        private void CreateWorld(EffectConfig<TestEffectsWorld, TestEffectMarker> config)
        {
            _world = new StaticEcsTestWorld<TestEffectsWorld>();
            EffectTypeRegistration.Register<TestEffectsWorld, TestEffectMarker>(_world.Types);
            _handler = new RecordingEffectHandler<TestEffectsWorld, TestEffectMarker>();

            new EffectsCoreFeature<TestEffectsWorld>().InstallResourcesAndRegisterTypesForTest(_world);
            World<TestEffectsWorld>.SetResource(EcsTime.Default());
            World<TestEffectsWorld>.SetResource<IEffectHandler<TestEffectsWorld, TestEffectMarker>>(_handler);
            World<TestEffectsWorld>.SetResource(config);
            new EffectFeature<TestEffectsWorld, TestEffectMarker>()
                .InstallResourcesAndRegisterTypesForTest(_world);
            _world.Initialize();
        }

        private static (World<TestEffectsWorld>.Entity Source, World<TestEffectsWorld>.Entity Target) CreatePair()
        {
            return (
                World<TestEffectsWorld>.NewEntity<Default>(),
                World<TestEffectsWorld>.NewEntity<Default>());
        }
    }
}
