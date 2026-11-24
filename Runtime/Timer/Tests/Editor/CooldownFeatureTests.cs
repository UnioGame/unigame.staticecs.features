namespace UniGame.StaticEcs.Features.Tests
{
    using FFS.Libraries.StaticEcs;
    using NUnit.Framework;
    using UniGame.StaticEcs.Tests;
    using UniGame.StaticEcs.Time;

    public struct TestCooldownWorld : IWorldType
    {
    }

    [TestFixture]
    public class CooldownFeatureTests
    {
        private StaticEcsTestWorld<TestCooldownWorld> _world;

        [SetUp]
        public void SetUp()
        {
            _world = new StaticEcsTestWorld<TestCooldownWorld>();
            World<TestCooldownWorld>.SetResource(EcsTime.Default());
            new TimerFeature<TestCooldownWorld>().InstallResourcesAndRegisterTypesForTest(_world);
            _world.Initialize();
            World<TestCooldownWorld>.Systems<StaticEcsUpdateSystems>.Initialize();
        }

        [TearDown]
        public void TearDown()
        {
            _world?.Dispose();
        }

        private static void Tick(float now)
        {
            ref var time = ref World<TestCooldownWorld>.GetResource<EcsTime>();
            time.DeltaTime = System.Math.Max(0f, now - time.Now);
            time.Now = now;
            World<TestCooldownWorld>.Systems<StaticEcsUpdateSystems>.Update();
        }

        [Test]
        public void RequestStartsCooldownAndCompletionIsReportedOnce()
        {
            var entity = World<TestCooldownWorld>.NewEntity<Default>();
            entity.Set(new CooldownComponent { DurationMilliseconds = 2000u });
            Assert.That(entity.Has<CooldownRemainingComponent>(), Is.False);
            var receiver = World<TestCooldownWorld>.RegisterEventReceiver<CooldownFinishedEvent>();
            try
            {
                World<TestCooldownWorld>.SendEvent(new RestartCooldownRequest { Target = entity.GID });
                Tick(5f);
                Assert.That(entity.Has<CooldownActiveTag>(), Is.True);
                Assert.That(entity.Has<CooldownRemainingComponent>(), Is.True);
                Assert.That(entity.Read<CooldownRemainingComponent>().RemainingMilliseconds,
                    Is.EqualTo(2000u));

                Tick(6f);
                Assert.That(entity.Read<CooldownRemainingComponent>().RemainingMilliseconds,
                    Is.EqualTo(1000u));
                Tick(7f);
                Assert.That(entity.Has<CooldownActiveTag>(), Is.False);
                Assert.That(entity.Has<CooldownCompleteTag>(), Is.True);
                Assert.That(entity.Read<CooldownRemainingComponent>().RemainingMilliseconds,
                    Is.Zero);

                var count = 0;
                foreach (var item in receiver)
                {
                    Assert.That(item.Value.Entity, Is.EqualTo(entity.GID));
                    count++;
                }
                Assert.That(count, Is.EqualTo(1));

                Tick(8f);
                foreach (var _ in receiver)
                    count++;
                Assert.That(count, Is.EqualTo(1));
            }
            finally
            {
                World<TestCooldownWorld>.DeleteEventReceiver(ref receiver);
            }
        }

        [Test]
        public void RestartResetsRemainingAndUsesCurrentDuration()
        {
            var entity = World<TestCooldownWorld>.NewEntity<Default>();
            entity.Set(new CooldownComponent { DurationMilliseconds = 2000u });
            Assert.That(CooldownOperations.Restart<TestCooldownWorld>(entity.GID), Is.True);
            Tick(1f);
            Assert.That(entity.Read<CooldownRemainingComponent>().RemainingMilliseconds,
                Is.EqualTo(1000u));

            ref var timer = ref entity.Mut<CooldownComponent>();
            timer.DurationMilliseconds = 4000u;
            Assert.That(CooldownOperations.Restart<TestCooldownWorld>(entity.GID), Is.True);
            Tick(2f);
            Assert.That(entity.Read<CooldownRemainingComponent>().RemainingMilliseconds,
                Is.EqualTo(3000u));
            Assert.That(entity.Has<CooldownCompleteTag>(), Is.False);
        }

        [Test]
        public void UnchangedEcsTimePausesCooldown()
        {
            var entity = World<TestCooldownWorld>.NewEntity<Default>();
            entity.Set(new CooldownComponent { DurationMilliseconds = 1000u });
            CooldownOperations.Restart<TestCooldownWorld>(entity.GID);

            Tick(0.25f);
            Tick(0.25f);
            Assert.That(entity.Read<CooldownRemainingComponent>().RemainingMilliseconds,
                Is.EqualTo(750u));
            Assert.That(entity.Has<CooldownActiveTag>(), Is.True);

            Tick(1f);
            Assert.That(entity.Has<CooldownCompleteTag>(), Is.True);
        }

        [Test]
        public void AutoRestartStartsNextUpdateWithoutCatchUp()
        {
            var entity = World<TestCooldownWorld>.NewEntity<Default>();
            entity.Set(new CooldownComponent { DurationMilliseconds = 1000u });
            entity.Set<CooldownAutoRestartTag>();
            CooldownOperations.Restart<TestCooldownWorld>(entity.GID);

            Tick(1f);
            Assert.That(entity.Has<CooldownCompleteTag>(), Is.True);
            Tick(5f);
            Assert.That(entity.Has<CooldownActiveTag>(), Is.True);
            Assert.That(entity.Read<CooldownRemainingComponent>().RemainingMilliseconds,
                Is.EqualTo(1000u));
            Tick(6f);
            Assert.That(entity.Has<CooldownCompleteTag>(), Is.True);
        }

        [Test]
        public void NonPositiveDurationFinishesOncePerRun()
        {
            var entity = World<TestCooldownWorld>.NewEntity<Default>();
            entity.Set(new CooldownComponent { DurationMilliseconds = 0u });
            entity.Set<CooldownAutoRestartTag>();
            var receiver = World<TestCooldownWorld>.RegisterEventReceiver<CooldownFinishedEvent>();
            try
            {
                CooldownOperations.Restart<TestCooldownWorld>(entity.GID);
                Tick(0f);
                var count = 0;
                foreach (var _ in receiver)
                    count++;
                Assert.That(count, Is.EqualTo(1));

                Tick(0f);
                foreach (var _ in receiver)
                    count++;
                Assert.That(count, Is.EqualTo(1));

                Tick(0f);
                foreach (var _ in receiver)
                    count++;
                Assert.That(count, Is.EqualTo(2));
            }
            finally
            {
                World<TestCooldownWorld>.DeleteEventReceiver(ref receiver);
            }
        }

        [Test]
        public void DestroyOnCompleteWinsOverAutoRestart()
        {
            var entity = World<TestCooldownWorld>.NewEntity<Default>();
            var gid = entity.GID;
            entity.Set(new CooldownComponent { DurationMilliseconds = 1000u });
            entity.Set<CooldownAutoRestartTag>();
            entity.Set<CooldownDestroyOnCompleteTag>();
            CooldownOperations.Restart<TestCooldownWorld>(gid);

            Tick(1f);
            Assert.That(gid.TryUnpack<TestCooldownWorld>(out _), Is.False);
            Tick(2f);
        }

        [Test]
        public void StaleRestartRequestIsIgnored()
        {
            var entity = World<TestCooldownWorld>.NewEntity<Default>();
            var gid = entity.GID;
            entity.Set(new CooldownComponent { DurationMilliseconds = 1000u });
            entity.Destroy();

            World<TestCooldownWorld>.SendEvent(new RestartCooldownRequest { Target = gid });
            Assert.DoesNotThrow(() => Tick(1f));
        }

        [Test]
        public void SerializableConverterStartsConfiguredCooldown()
        {
            var entity = World<TestCooldownWorld>.NewEntity<Default>();
            var converter = new CooldownSerializableConverter<TestCooldownWorld>
            {
                duration = 3f,
                autoRestart = true,
                destroyOnComplete = true,
            };

            converter.Apply(entity, null);

            Assert.That(entity.Has<CooldownActiveTag>(), Is.True);
            Assert.That(entity.Has<CooldownAutoRestartTag>(), Is.True);
            Assert.That(entity.Has<CooldownDestroyOnCompleteTag>(), Is.True);
            Assert.That(entity.Read<CooldownRemainingComponent>().RemainingMilliseconds,
                Is.EqualTo(3000u));
        }

        [Test]
        public void ThirtyMinutesOfFixedStepsHaveNoAccumulatedRoundingError()
        {
            Assert.That(TimerMillisecondOperations.FromSeconds(1800f),
                Is.EqualTo(1_800_000u));
            var remaining = 1_800_000u;
            var fraction = 0f;
            for (var tick = 0; tick < 36_000; tick++)
                remaining -= TimerMillisecondOperations.TakeStep(0.05f, ref fraction);

            Assert.That(remaining, Is.Zero);
            Assert.That(fraction, Is.Zero);
        }

        [Test]
        public void RemovingCooldownClearsAllStateBeforeReaddingIt()
        {
            var entity = World<TestCooldownWorld>.NewEntity<Default>();
            entity.Set(new CooldownComponent { DurationMilliseconds = 1000u });
            entity.Set<CooldownAutoRestartTag>();
            entity.Set<CooldownDestroyOnCompleteTag>();
            CooldownOperations.Restart<TestCooldownWorld>(entity.GID);
            entity.Delete<CooldownComponent>();
            Tick(0f);

            Assert.That(entity.Has<CooldownRemainingComponent>(), Is.False);
            Assert.That(entity.Has<CooldownActiveTag>(), Is.False);
            Assert.That(entity.Has<CooldownAutoRestartTag>(), Is.False);
            Assert.That(entity.Has<CooldownDestroyOnCompleteTag>(), Is.False);

            entity.Set<CooldownCompleteTag>();
            Tick(0f);
            Assert.That(entity.Has<CooldownCompleteTag>(), Is.False);

            entity.Set(new CooldownComponent { DurationMilliseconds = 1000u });
            Tick(1f);
            Assert.That(entity.Has<CooldownRemainingComponent>(), Is.False);
            Assert.That(entity.Has<CooldownActiveTag>(), Is.False);
        }
    }
}
