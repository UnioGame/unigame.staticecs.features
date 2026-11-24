namespace UniGame.StaticEcs.Features.Tests
{
    using FFS.Libraries.StaticEcs;
    using NUnit.Framework;
    using UniGame.StaticEcs.Tests;
    using UniGame.StaticEcs.Time;

    public struct TestTimerWorld : IWorldType
    {
    }

    [TestFixture]
    public class TimerFeatureTests
    {
        private StaticEcsTestWorld<TestTimerWorld> _world;
        private EventReceiver<TestTimerWorld, TimerFinishedEvent> _finished;

        [SetUp]
        public void SetUp()
        {
            _world = new StaticEcsTestWorld<TestTimerWorld>();
            World<TestTimerWorld>.SetResource(EcsTime.Default());
            new TimerFeature<TestTimerWorld>().InstallResourcesAndRegisterTypesForTest(_world);
            _world.Initialize();
            World<TestTimerWorld>.Systems<StaticEcsUpdateSystems>.Initialize();
            _finished = World<TestTimerWorld>.RegisterEventReceiver<TimerFinishedEvent>();
        }

        [TearDown]
        public void TearDown()
        {
            World<TestTimerWorld>.DeleteEventReceiver(ref _finished);
            _world?.Dispose();
        }

        private static World<TestTimerWorld>.Entity Create(uint limit = 0u)
        {
            var entity = World<TestTimerWorld>.NewEntity<Default>();
            entity.Set(new TimerComponent { LimitMilliseconds = limit });
            return entity;
        }

        private static void Tick(float now)
        {
            ref var time = ref World<TestTimerWorld>.GetResource<EcsTime>();
            time.DeltaTime = System.Math.Max(0f, now - time.Now);
            time.Now = now;
            World<TestTimerWorld>.Systems<StaticEcsUpdateSystems>.Update();
        }

        private int CountFinished(EntityGID target)
        {
            var count = 0;
            foreach (var item in _finished)
            {
                Assert.That(item.Value.Entity, Is.EqualTo(target));
                count++;
            }
            return count;
        }

        [Test]
        public void UnlimitedTimerAccumulatesWithoutCompletion()
        {
            var entity = Create();
            Assert.That(TimerOperations.Start<TestTimerWorld>(entity.GID), Is.True);
            Tick(1f);
            Tick(7f);
            Assert.That(entity.Read<TimerComponent>().ElapsedMilliseconds, Is.EqualTo(7000u));
            Assert.That(entity.Has<TimerActiveTag>(), Is.True);
            Assert.That(CountFinished(entity.GID), Is.Zero);
        }

        [Test]
        public void PausePreservesLastTickProgressAndStartContinuesWithoutPausedTime()
        {
            var entity = Create();
            TimerOperations.Start<TestTimerWorld>(entity.GID);
            Tick(1f);
            Assert.That(TimerOperations.Pause<TestTimerWorld>(entity.GID), Is.True);
            Assert.That(entity.Read<TimerComponent>().ElapsedMilliseconds, Is.EqualTo(1000u));
            Tick(10f);
            Assert.That(entity.Read<TimerComponent>().ElapsedMilliseconds, Is.EqualTo(1000u));
            TimerOperations.Start<TestTimerWorld>(entity.GID);
            Tick(10.5f);
            Assert.That(entity.Read<TimerComponent>().ElapsedMilliseconds, Is.EqualTo(1500u));
        }

        [Test]
        public void ResetPausesAndRestartBeginsFromZero()
        {
            var entity = Create();
            TimerOperations.Start<TestTimerWorld>(entity.GID);
            Tick(2f);
            Assert.That(TimerOperations.Reset<TestTimerWorld>(entity.GID), Is.True);
            Assert.That(entity.Read<TimerComponent>().ElapsedMilliseconds, Is.Zero);
            Assert.That(entity.Has<TimerActiveTag>(), Is.False);
            Tick(5f);
            Assert.That(entity.Read<TimerComponent>().ElapsedMilliseconds, Is.Zero);
            TimerOperations.Restart<TestTimerWorld>(entity.GID);
            Tick(6f);
            Assert.That(entity.Read<TimerComponent>().ElapsedMilliseconds, Is.EqualTo(1000u));
        }

        [TestCase(5000u)]
        [TestCase(6000u)]
        public void LimitClampsElapsedAndFinishesOnce(uint limit)
        {
            var entity = Create(limit);
            TimerOperations.Start<TestTimerWorld>(entity.GID);
            Tick(20f);
            Assert.That(CountFinished(entity.GID), Is.EqualTo(1));
            Assert.That(entity.Read<TimerComponent>().ElapsedMilliseconds, Is.EqualTo(limit));
            Assert.That(entity.Has<TimerCompleteTag>(), Is.True);
            Assert.That(entity.Has<TimerActiveTag>(), Is.False);
            Assert.That(TimerOperations.Start<TestTimerWorld>(entity.GID), Is.False);
            Tick(21f);
            Assert.That(CountFinished(entity.GID), Is.Zero);
            Assert.That(TimerOperations.Reset<TestTimerWorld>(entity.GID), Is.True);
            Assert.That(TimerOperations.Start<TestTimerWorld>(entity.GID), Is.True);
        }

        [Test]
        public void PauseBeforeUpdateDoesNotApplyUnprocessedTime()
        {
            var entity = Create(2000u);
            TimerOperations.Start<TestTimerWorld>(entity.GID);
            Tick(1f);
            TimerOperations.Pause<TestTimerWorld>(entity.GID);
            Tick(3f);
            Assert.That(entity.Read<TimerComponent>().ElapsedMilliseconds, Is.EqualTo(1000u));
            Assert.That(CountFinished(entity.GID), Is.Zero);
            TimerOperations.Start<TestTimerWorld>(entity.GID);
            Tick(4f);
            Assert.That(entity.Read<TimerComponent>().ElapsedMilliseconds, Is.EqualTo(2000u));
            Assert.That(entity.Has<TimerCompleteTag>(), Is.True);
            Assert.That(CountFinished(entity.GID), Is.EqualTo(1));
        }

        [Test]
        public void ConverterConfiguresAndStartsTimer()
        {
            var entity = World<TestTimerWorld>.NewEntity<Default>();
            var converter = new TimerSerializableConverter<TestTimerWorld>
            {
                limitSeconds = 5f
            };
            converter.Apply(entity, null);
            Assert.That(entity.Has<TimerActiveTag>(), Is.True);
            Assert.That(entity.Read<TimerComponent>().LimitMilliseconds, Is.EqualTo(5000u));
        }

        [Test]
        public void RequestsApplyPauseResetAndRestart()
        {
            var entity = Create();
            World<TestTimerWorld>.SendEvent(new TimerControlRequest
            {
                Target = entity.GID,
                Action = TimerAction.Start
            });
            Tick(0f);
            Tick(0.5f);

            World<TestTimerWorld>.SendEvent(new TimerControlRequest
            {
                Target = entity.GID,
                Action = TimerAction.Pause
            });
            Tick(1f);
            Assert.That(entity.Read<TimerComponent>().ElapsedMilliseconds, Is.EqualTo(1000u));
            Assert.That(entity.Has<TimerActiveTag>(), Is.False);

            World<TestTimerWorld>.SendEvent(new TimerControlRequest
            {
                Target = entity.GID,
                Action = TimerAction.Reset
            });
            Tick(2f);
            Assert.That(entity.Read<TimerComponent>().ElapsedMilliseconds, Is.Zero);

            World<TestTimerWorld>.SendEvent(new TimerControlRequest
            {
                Target = entity.GID,
                Action = TimerAction.Restart
            });
            Tick(3f);
            Tick(4f);
            Assert.That(entity.Has<TimerActiveTag>(), Is.True);
            Assert.That(entity.Read<TimerComponent>().ElapsedMilliseconds, Is.EqualTo(1000u));
        }

        [Test]
        public void StartRequestBeginsCountingOnTheNextUpdate()
        {
            var entity = Create();
            World<TestTimerWorld>.SendEvent(new TimerControlRequest
            {
                Target = entity.GID,
                Action = TimerAction.Start
            });

            Tick(5f);
            Assert.That(entity.Has<TimerActiveTag>(), Is.True);
            Assert.That(entity.Read<TimerComponent>().ElapsedMilliseconds, Is.Zero);

            Tick(6f);
            Assert.That(entity.Read<TimerComponent>().ElapsedMilliseconds, Is.EqualTo(1000u));
        }

        [Test]
        public void ControlRequestAndCooldownWorkOnSameEntity()
        {
            var entity = Create();
            entity.Set(new CooldownComponent { DurationMilliseconds = 3000u });
            CooldownOperations.Restart<TestTimerWorld>(entity.GID);
            World<TestTimerWorld>.SendEvent(new TimerControlRequest
            {
                Target = entity.GID,
                Action = TimerAction.Start
            });
            Tick(0f);
            Tick(2f);
            Assert.That(entity.Read<CooldownRemainingComponent>().RemainingMilliseconds,
                Is.EqualTo(1000u));
            Assert.That(entity.Read<TimerComponent>().ElapsedMilliseconds, Is.EqualTo(2000u));
            Tick(3f);
            Assert.That(entity.Has<CooldownCompleteTag>(), Is.True);
            Assert.That(entity.Has<TimerActiveTag>(), Is.True);
            Assert.That(CountFinished(entity.GID), Is.Zero);
        }

        [Test]
        public void TwoDaysOfFixedStepsHaveNoAccumulatedRoundingError()
        {
            Assert.That(TimerMillisecondOperations.FromSeconds(172_800f),
                Is.EqualTo(172_800_000u));
            Assert.That(TimerMillisecondOperations.FromSeconds(0.05f),
                Is.EqualTo(50u));
            var elapsed = 0u;
            var fraction = 0f;
            for (var tick = 0; tick < 3_456_000; tick++)
                elapsed += TimerMillisecondOperations.TakeStep(0.05f, ref fraction);

            Assert.That(elapsed, Is.EqualTo(172_800_000u));
            Assert.That(fraction, Is.Zero);
        }

        [Test]
        public void FractionalMillisecondsAreCarriedBetweenFrames()
        {
            var fraction = 0f;
            var elapsed = 0u;
            for (var frame = 0; frame < 4; frame++)
                elapsed += TimerMillisecondOperations.TakeStep(0.00025f, ref fraction);

            Assert.That(elapsed, Is.EqualTo(1u));
            Assert.That(fraction, Is.Zero);
        }

        [Test]
        public void RemovingTimerClearsStateBeforeReaddingIt()
        {
            var entity = Create(1000u);
            TimerOperations.Start<TestTimerWorld>(entity.GID);
            entity.Delete<TimerComponent>();
            Tick(0f);
            Assert.That(entity.Has<TimerActiveTag>(), Is.False);

            entity.Set<TimerCompleteTag>();
            Tick(0f);
            Assert.That(entity.Has<TimerCompleteTag>(), Is.False);

            entity.Set(new TimerComponent { LimitMilliseconds = 1000u });
            Tick(1f);
            Assert.That(entity.Read<TimerComponent>().ElapsedMilliseconds, Is.Zero);
            Assert.That(entity.Has<TimerActiveTag>(), Is.False);
        }
    }
}
