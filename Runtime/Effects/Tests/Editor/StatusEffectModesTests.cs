namespace UniGame.StaticEcs.Features.Tests
{
    using FFS.Libraries.StaticEcs;
    using NUnit.Framework;
    using UniGame.StaticEcs.Tests;
    using UniGame.StaticEcs.Time;

    [TestFixture]
    public sealed class StatusEffectModesTests
    {
        private RecordingEffectHandler<TestEffectsWorld, TestEffectMarker> _handlerA;
        private RecordingEffectHandler<TestEffectsWorld, TestEffectMarkerB> _handlerB;
        private EffectTickSystem<TestEffectsWorld, TestEffectMarker> _systemA;
        private StaticEcsTestWorld<TestEffectsWorld> _world;

        [TearDown]
        public void TearDown()
        {
            _world?.Dispose();
            _world = null;
        }

        [Test]
        public void TimeTick_ThreeSeconds_FiresExactlyThreeTicksAndExpires()
        {
            CreateWorld(Settings(EffectDurationMode.Time, EffectActivationMode.Tick));
            var (source, target) = CreatePair();
            EffectOperations.Apply<TestEffectsWorld, TestEffectMarker>(
                target.GID, source.GID, new EffectApplyParams { Duration = 3f });

            for (var second = 1; second <= 3; second++)
            {
                Step(1f);
                Assert.AreEqual(second, _handlerA.Ticks.Count, $"ticks after {second}s");
            }

            Assert.IsFalse(target.Has<EffectComponent<TestEffectMarker>>());
            Assert.AreEqual(1, _handlerA.Removed.Count);
            Assert.IsTrue(_handlerA.Removed[0].Expired);
        }

        [Test]
        public void TimeTick_WithFrameSizedSteps_StillFiresExactlyThreeTicks()
        {
            CreateWorld(Settings(EffectDurationMode.Time, EffectActivationMode.Tick));
            var (source, target) = CreatePair();
            EffectOperations.Apply<TestEffectsWorld, TestEffectMarker>(
                target.GID, source.GID, new EffectApplyParams { Duration = 3f });

            for (var frame = 0; frame < 240; frame++)
                Step(1f / 60f);

            Assert.AreEqual(3, _handlerA.Ticks.Count);
            Assert.IsFalse(target.Has<EffectComponent<TestEffectMarker>>());
        }

        [Test]
        public void TimeTick_IgnoresPeriodOverride()
        {
            CreateWorld(Settings(EffectDurationMode.Time, EffectActivationMode.Tick));
            var (source, target) = CreatePair();
            EffectOperations.Apply<TestEffectsWorld, TestEffectMarker>(
                target.GID, source.GID, new EffectApplyParams { Duration = 3f, Period = 0.25f });

            Step(1f);

            Assert.AreEqual(1, _handlerA.Ticks.Count);
        }

        [Test]
        public void TimeCustomInterval_UsesPeriodOverride()
        {
            CreateWorld(Settings(EffectDurationMode.Time, EffectActivationMode.CustomInterval));
            var (source, target) = CreatePair();
            EffectOperations.Apply<TestEffectsWorld, TestEffectMarker>(
                target.GID, source.GID, new EffectApplyParams { Duration = 2f, Period = 0.5f });

            for (var i = 0; i < 4; i++)
                Step(0.5f);

            Assert.AreEqual(4, _handlerA.Ticks.Count);
            Assert.IsFalse(target.Has<EffectComponent<TestEffectMarker>>());
        }

        [Test]
        public void TimeCustomInterval_FallsBackToSettingsPeriod()
        {
            CreateWorld(Settings(EffectDurationMode.Time, EffectActivationMode.CustomInterval, period: 0.5f));
            var (source, target) = CreatePair();
            EffectOperations.Apply<TestEffectsWorld, TestEffectMarker>(
                target.GID, source.GID, new EffectApplyParams { Duration = 2f });

            Step(1f);

            Assert.AreEqual(2, _handlerA.Ticks.Count);
        }

        [Test]
        public void ChargesTick_SpendsOneChargePerTickAndExpiresWhenSpent()
        {
            CreateWorld(Settings(EffectDurationMode.Charges, EffectActivationMode.Tick));
            var (source, target) = CreatePair();
            EffectOperations.Apply<TestEffectsWorld, TestEffectMarker>(
                target.GID, source.GID, new EffectApplyParams { Charges = 2 });

            Step(1f);
            Assert.AreEqual(1, _handlerA.Ticks.Count);
            Assert.AreEqual(1, target.Read<EffectComponent<TestEffectMarker>>().ChargesLeft);

            Step(1f);
            Assert.AreEqual(2, _handlerA.Ticks.Count);
            Assert.IsFalse(target.Has<EffectComponent<TestEffectMarker>>());
            Assert.IsTrue(_handlerA.Removed[0].Expired);
        }

        [Test]
        public void ChargesEffect_DoesNotExpireByTimeWhileChargesRemain()
        {
            CreateWorld(Settings(EffectDurationMode.Charges, EffectActivationMode.CustomInterval, period: 100f));
            var (source, target) = CreatePair();
            EffectOperations.Apply<TestEffectsWorld, TestEffectMarker>(
                target.GID, source.GID, new EffectApplyParams { Charges = 2 });

            Step(50f);

            Assert.IsTrue(target.Has<EffectComponent<TestEffectMarker>>());
            Assert.AreEqual(0, _handlerA.Ticks.Count);
        }

        [Test]
        public void ChargesWithoutPeriod_IsRejected()
        {
            CreateWorld(Settings(EffectDurationMode.Charges, EffectActivationMode.CustomInterval, period: 0f));
            var (source, target) = CreatePair();

            var result = EffectOperations.Apply<TestEffectsWorld, TestEffectMarker>(
                target.GID, source.GID, new EffectApplyParams { Charges = 2 });

            Assert.AreEqual(EffectApplyResult.Failed, result);
            Assert.IsFalse(target.Has<EffectComponent<TestEffectMarker>>());
        }

        [Test]
        public void ChargesCondition_SpendsChargeOnlyWhileConditionIsMet()
        {
            var condition = new ToggleCondition();
            CreateWorld(Settings(EffectDurationMode.Charges, EffectActivationMode.Condition), condition: condition);
            var (source, target) = CreatePair();
            EffectOperations.Apply<TestEffectsWorld, TestEffectMarker>(
                target.GID, source.GID, new EffectApplyParams { Charges = 2 });

            Step(0.1f);
            Assert.AreEqual(0, _handlerA.Ticks.Count);

            condition.Met = true;
            Step(0.1f);
            Assert.AreEqual(1, _handlerA.Ticks.Count);

            Step(0.1f);
            Assert.AreEqual(2, _handlerA.Ticks.Count);
            Assert.IsFalse(target.Has<EffectComponent<TestEffectMarker>>());
        }

        [Test]
        public void Reapply_Time_AddsDurationUpToMaxAndKeepsMagnitudeAndPhase()
        {
            CreateWorld(Settings(EffectDurationMode.Time, EffectActivationMode.Tick));
            var (source, target) = CreatePair();
            EffectOperations.Apply<TestEffectsWorld, TestEffectMarker>(
                target.GID, source.GID, new EffectApplyParams { Duration = 3f, Magnitude = 5f });
            Step(0.5f);

            var result = EffectOperations.Apply<TestEffectsWorld, TestEffectMarker>(
                target.GID, source.GID, new EffectApplyParams { Duration = 3f, Magnitude = 9f });

            Assert.AreEqual(EffectApplyResult.Refreshed, result);
            var effect = target.Read<EffectComponent<TestEffectMarker>>();
            Assert.AreEqual(5f, effect.TimeLeft, 0.0001f, "2.5s left + 3s is capped by the 5s maximum");
            Assert.AreEqual(5f, effect.Magnitude, "reapplying does not change the damage");
            Assert.AreEqual(0.5f, effect.PeriodLeft, 0.0001f, "reapplying keeps the activation phase");
        }

        [Test]
        public void Apply_Time_ClampsDurationToMaximum()
        {
            CreateWorld(Settings(EffectDurationMode.Time, EffectActivationMode.Tick));
            var (source, target) = CreatePair();

            EffectOperations.Apply<TestEffectsWorld, TestEffectMarker>(
                target.GID, source.GID, new EffectApplyParams { Duration = 100f });

            Assert.AreEqual(5f, target.Read<EffectComponent<TestEffectMarker>>().TimeLeft);
        }

        [Test]
        public void Reapply_Charges_AddsChargesUpToMax()
        {
            CreateWorld(Settings(EffectDurationMode.Charges, EffectActivationMode.Tick));
            var (source, target) = CreatePair();
            EffectOperations.Apply<TestEffectsWorld, TestEffectMarker>(
                target.GID, source.GID, new EffectApplyParams { Charges = 3 });

            EffectOperations.Apply<TestEffectsWorld, TestEffectMarker>(
                target.GID, source.GID, new EffectApplyParams { Charges = 4 });

            Assert.AreEqual(5, target.Read<EffectComponent<TestEffectMarker>>().ChargesLeft);
        }

        [Test]
        public void Apply_StoresMagnitudeAndSourceNameAsHistory()
        {
            CreateWorld(Settings(EffectDurationMode.Time, EffectActivationMode.Tick));
            var (source, target) = CreatePair();

            EffectOperations.Apply<TestEffectsWorld, TestEffectMarker>(
                target.GID,
                source.GID,
                new EffectApplyParams { Duration = 3f, Magnitude = 7f, SourceName = "Spider" });

            var effect = target.Read<EffectComponent<TestEffectMarker>>();
            Assert.AreEqual(7f, effect.Magnitude);
            Assert.AreEqual("Spider", effect.SourceName);
            Assert.AreEqual(source.GID, (EntityGID)effect.Source);
        }

        [Test]
        public void DestroyingSource_KeepsEffectWhenRemoveWithSourceIsOff()
        {
            var settings = Settings(EffectDurationMode.Time, EffectActivationMode.Tick);
            settings.removeWithSource = false;
            CreateWorld(settings);
            var (source, target) = CreatePair();
            EffectOperations.Apply<TestEffectsWorld, TestEffectMarker>(
                target.GID, source.GID, new EffectApplyParams { Duration = 3f, Magnitude = 4f });

            source.Destroy();
            Step(1f);

            Assert.IsTrue(target.Has<EffectComponent<TestEffectMarker>>());
            Assert.AreEqual(1, _handlerA.Ticks.Count, "the effect keeps ticking after its source died");
        }

        [Test]
        public void RemoveWithSource_RegistersBackReferenceOnlyWhenOn()
        {
            var tracked = Settings(EffectDurationMode.Time, EffectActivationMode.Tick);
            tracked.removeWithSource = true;
            CreateWorld(tracked, Settings(EffectDurationMode.Time, EffectActivationMode.Tick));
            var (source, target) = CreatePair();
            var untrackedSource = World<TestEffectsWorld>.NewEntity<Default>();

            EffectOperations.Apply<TestEffectsWorld, TestEffectMarker>(
                target.GID, source.GID, new EffectApplyParams { Duration = 3f });
            EffectOperations.Apply<TestEffectsWorld, TestEffectMarkerB>(
                target.GID, untrackedSource.GID, new EffectApplyParams { Duration = 3f });

            Assert.IsTrue(source.Has<World<TestEffectsWorld>.Multi<EffectTargetComponent>>());
            Assert.IsFalse(untrackedSource.Has<World<TestEffectsWorld>.Multi<EffectTargetComponent>>());
        }

        [Test]
        public void Conflict_HigherPriorityReplacesLowerOne()
        {
            var weak = Settings(EffectDurationMode.Time, EffectActivationMode.Tick);
            weak.conflictGroup = "Poison";
            weak.conflictPriority = 0;
            var strong = Settings(EffectDurationMode.Time, EffectActivationMode.Tick);
            strong.conflictGroup = "Poison";
            strong.conflictPriority = 1;
            CreateWorld(weak, strong);
            var (source, target) = CreatePair();
            EffectOperations.Apply<TestEffectsWorld, TestEffectMarker>(
                target.GID, source.GID, new EffectApplyParams { Duration = 3f });

            var result = EffectOperations.Apply<TestEffectsWorld, TestEffectMarkerB>(
                target.GID, source.GID, new EffectApplyParams { Duration = 3f });

            Assert.AreEqual(EffectApplyResult.Applied, result);
            Assert.IsFalse(target.Has<EffectComponent<TestEffectMarker>>());
            Assert.IsTrue(target.Has<EffectComponent<TestEffectMarkerB>>());
            Assert.AreEqual(1, _handlerA.Removed.Count);
            Assert.IsFalse(_handlerA.Removed[0].Expired);
        }

        [Test]
        public void Conflict_LowerPriorityIsRejectedWhileHigherOneIsActive()
        {
            var weak = Settings(EffectDurationMode.Time, EffectActivationMode.Tick);
            weak.conflictGroup = "Poison";
            weak.conflictPriority = 0;
            var strong = Settings(EffectDurationMode.Time, EffectActivationMode.Tick);
            strong.conflictGroup = "Poison";
            strong.conflictPriority = 1;
            CreateWorld(weak, strong);
            var (source, target) = CreatePair();
            EffectOperations.Apply<TestEffectsWorld, TestEffectMarkerB>(
                target.GID, source.GID, new EffectApplyParams { Duration = 3f });

            var result = EffectOperations.Apply<TestEffectsWorld, TestEffectMarker>(
                target.GID, source.GID, new EffectApplyParams { Duration = 3f });

            Assert.AreEqual(EffectApplyResult.ConflictRejected, result);
            Assert.IsFalse(target.Has<EffectComponent<TestEffectMarker>>());
            Assert.IsTrue(target.Has<EffectComponent<TestEffectMarkerB>>());
        }

        [Test]
        public void Conflict_EqualPriorityIsReplacedByTheNewEffect()
        {
            var first = Settings(EffectDurationMode.Time, EffectActivationMode.Tick);
            first.conflictGroup = "Poison";
            var second = Settings(EffectDurationMode.Time, EffectActivationMode.Tick);
            second.conflictGroup = "Poison";
            CreateWorld(first, second);
            var (source, target) = CreatePair();
            EffectOperations.Apply<TestEffectsWorld, TestEffectMarker>(
                target.GID, source.GID, new EffectApplyParams { Duration = 3f });

            EffectOperations.Apply<TestEffectsWorld, TestEffectMarkerB>(
                target.GID, source.GID, new EffectApplyParams { Duration = 3f });

            Assert.IsFalse(target.Has<EffectComponent<TestEffectMarker>>());
            Assert.IsTrue(target.Has<EffectComponent<TestEffectMarkerB>>());
        }

        [Test]
        public void Conflict_DifferentGroupsCoexist()
        {
            var first = Settings(EffectDurationMode.Time, EffectActivationMode.Tick);
            first.conflictGroup = "Poison";
            var second = Settings(EffectDurationMode.Time, EffectActivationMode.Tick);
            second.conflictGroup = "Fire";
            CreateWorld(first, second);
            var (source, target) = CreatePair();
            EffectOperations.Apply<TestEffectsWorld, TestEffectMarker>(
                target.GID, source.GID, new EffectApplyParams { Duration = 3f });

            EffectOperations.Apply<TestEffectsWorld, TestEffectMarkerB>(
                target.GID, source.GID, new EffectApplyParams { Duration = 3f });

            Assert.IsTrue(target.Has<EffectComponent<TestEffectMarker>>());
            Assert.IsTrue(target.Has<EffectComponent<TestEffectMarkerB>>());
        }

        private static StatusEffectSettings Settings(
            EffectDurationMode duration,
            EffectActivationMode activation,
            float seconds = 3f,
            int charges = 3,
            float period = 1f)
        {
            return new StatusEffectSettings
            {
                durationMode = duration,
                duration = seconds,
                maxDuration = 5f,
                charges = charges,
                maxCharges = 5,
                activationMode = activation,
                period = period,
            };
        }

        private void CreateWorld(
            StatusEffectSettings settingsA,
            StatusEffectSettings settingsB = null,
            IEffectCondition<TestEffectsWorld, TestEffectMarker> condition = null)
        {
            _world = new StaticEcsTestWorld<TestEffectsWorld>();
            var types = _world.Types;
            EffectTypeRegistration.Register<TestEffectsWorld, TestEffectMarker>(types);
            EffectTypeRegistration.Register<TestEffectsWorld, TestEffectMarkerB>(types);
            _handlerA = new RecordingEffectHandler<TestEffectsWorld, TestEffectMarker>();
            _handlerB = new RecordingEffectHandler<TestEffectsWorld, TestEffectMarkerB>();

            new EffectsCoreFeature<TestEffectsWorld>().InstallResourcesAndRegisterTypesForTest(_world);
            World<TestEffectsWorld>.SetResource(EcsTime.Default());
            World<TestEffectsWorld>.SetResource<IEffectHandler<TestEffectsWorld, TestEffectMarker>>(_handlerA);
            World<TestEffectsWorld>.SetResource<IEffectHandler<TestEffectsWorld, TestEffectMarkerB>>(_handlerB);
            World<TestEffectsWorld>.SetResource(
                new EffectConfig<TestEffectsWorld, TestEffectMarker>(settingsA, registerTickSystem: false));
            World<TestEffectsWorld>.SetResource(
                new EffectConfig<TestEffectsWorld, TestEffectMarkerB>(
                    settingsB ?? settingsA,
                    registerTickSystem: false));
            if (condition != null)
                World<TestEffectsWorld>.SetResource(condition);

            new EffectFeature<TestEffectsWorld, TestEffectMarker>()
                .InstallResourcesAndRegisterTypesForTest(_world);
            new EffectFeature<TestEffectsWorld, TestEffectMarkerB>()
                .InstallResourcesAndRegisterTypesForTest(_world);

            _world.Initialize();
            _systemA = new EffectTickSystem<TestEffectsWorld, TestEffectMarker>();
        }

        private static (World<TestEffectsWorld>.Entity Source, World<TestEffectsWorld>.Entity Target) CreatePair()
        {
            return (
                World<TestEffectsWorld>.NewEntity<Default>(),
                World<TestEffectsWorld>.NewEntity<Default>());
        }

        private void Step(float dt)
        {
            ref var time = ref World<TestEffectsWorld>.GetResource<EcsTime>();
            time.DeltaTime = dt;
            _systemA.Update();
        }

        private sealed class ToggleCondition : IEffectCondition<TestEffectsWorld, TestEffectMarker>
        {
            public bool Met;

            public bool IsMet(EntityGID target, EntityGID source) => Met;
        }
    }
}
