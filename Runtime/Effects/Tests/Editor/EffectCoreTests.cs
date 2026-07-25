namespace UniGame.StaticEcs.Features.Tests
{
    using FFS.Libraries.StaticEcs;
    using NUnit.Framework;
    using UniGame.StaticEcs.Tests;
    using UniGame.StaticEcs.Time;

    [TestFixture]
    public sealed class EffectCoreTests
    {
        private RecordingEffectHandler<TestEffectsWorld, TestEffectMarker> _handler;
        private EffectTickSystem<TestEffectsWorld, TestEffectMarker> _system;
        private StaticEcsTestWorld<TestEffectsWorld> _world;

        [SetUp]
        public void SetUp()
        {
            _world = new StaticEcsTestWorld<TestEffectsWorld>();
            EffectTypeRegistration.Register<TestEffectsWorld, TestEffectMarker>(
                _world.Types);
            _handler = new RecordingEffectHandler<TestEffectsWorld, TestEffectMarker>();

            new EffectsCoreFeature<TestEffectsWorld>().InstallResourcesAndRegisterTypesForTest(
                _world
            );
            new EcsTimeFeature<TestEffectsWorld>().InstallResourcesAndRegisterTypesForTest(
                _world
            );
            World<TestEffectsWorld>.SetResource<
                IEffectHandler<TestEffectsWorld, TestEffectMarker>>(_handler);
            var config = new EffectConfig<TestEffectsWorld, TestEffectMarker>(
                maxStacks: 3,
                refreshOnReapply: true,
                registerTickSystem: false);
            World<TestEffectsWorld>.SetResource(config);
            new EffectFeature<TestEffectsWorld, TestEffectMarker>()
                .InstallResourcesAndRegisterTypesForTest(_world);

            _world.Initialize();
            _system = new EffectTickSystem<TestEffectsWorld, TestEffectMarker>();
        }

        [TearDown]
        public void TearDown()
        {
            _world?.Dispose();
        }

        private static void Tick(float dt)
        {
            ref var time = ref World<TestEffectsWorld>.GetResource<EcsTime>();
            time.DeltaTime = dt;
        }

        [Test]
        public void Apply_AddsComponentAndRoster_RaisesAppliedEvent()
        {
            var receiver = World<TestEffectsWorld>.RegisterEventReceiver<
                EffectAppliedEvent<TestEffectMarker>
            >();
            try
            {
                var source = World<TestEffectsWorld>.NewEntity<Default>();
                var target = World<TestEffectsWorld>.NewEntity<Default>();

                Assert.IsTrue(
                    EffectOperations.Apply<TestEffectsWorld, TestEffectMarker>(
                        target.GID,
                        source.GID,
                        duration: 5f
                    )
                );

                Assert.IsTrue(target.Has<EffectComponent<TestEffectMarker>>());
                Assert.AreEqual(5f, target.Read<EffectComponent<TestEffectMarker>>().TimeLeft);
                Assert.AreEqual(1, target.Read<EffectComponent<TestEffectMarker>>().Stacks);

                Assert.IsTrue(target.Has<World<TestEffectsWorld>.Multi<EffectSummaryComponent>>());
                var roster = target.Read<World<TestEffectsWorld>.Multi<EffectSummaryComponent>>();
                Assert.AreEqual(1, roster.Length);
                Assert.AreEqual(1, roster[0].Stacks);

                var count = 0;
                foreach (var _ in receiver)
                {
                    count++;
                }

                Assert.AreEqual(1, count);
                Assert.AreEqual(1, _handler.Applied.Count);
                Assert.AreEqual(0, _handler.Applied[0].PreviousStacks);
            }
            finally
            {
                World<TestEffectsWorld>.DeleteEventReceiver(ref receiver);
            }
        }

        [Test]
        public void Remove_DeletesComponentAndRoster_RaisesRemovedEvent()
        {
            var receiver = World<TestEffectsWorld>.RegisterEventReceiver<
                EffectRemovedEvent<TestEffectMarker>
            >();
            try
            {
                var source = World<TestEffectsWorld>.NewEntity<Default>();
                var target = World<TestEffectsWorld>.NewEntity<Default>();
                EffectOperations.Apply<TestEffectsWorld, TestEffectMarker>(
                    target.GID,
                    source.GID,
                    duration: 5f
                );

                Assert.IsTrue(
                    EffectOperations.Remove<TestEffectsWorld, TestEffectMarker>(target.GID)
                );
                Assert.IsFalse(target.Has<EffectComponent<TestEffectMarker>>());
                var roster = target.Read<World<TestEffectsWorld>.Multi<EffectSummaryComponent>>();
                Assert.AreEqual(0, roster.Length);

                var found = false;
                foreach (var e in receiver)
                {
                    Assert.IsFalse(e.Value.Expired);
                    found = true;
                }

                Assert.IsTrue(found);
                Assert.AreEqual(1, _handler.Removed.Count);
                Assert.IsFalse(_handler.Removed[0].Expired);
            }
            finally
            {
                World<TestEffectsWorld>.DeleteEventReceiver(ref receiver);
            }
        }

        [Test]
        public void Tick_WithoutPeriod_DoesNotInvokeHandler_AndExpiresOnTimeOut()
        {
            var receiver = World<TestEffectsWorld>.RegisterEventReceiver<
                EffectRemovedEvent<TestEffectMarker>
            >();
            try
            {
                var source = World<TestEffectsWorld>.NewEntity<Default>();
                var target = World<TestEffectsWorld>.NewEntity<Default>();
                EffectOperations.Apply<TestEffectsWorld, TestEffectMarker>(
                    target.GID,
                    source.GID,
                    duration: 1f
                );

                Tick(0.4f);
                _system.Update();
                Assert.IsTrue(target.Has<EffectComponent<TestEffectMarker>>());
                Assert.AreEqual(0, _handler.Ticks.Count);

                Tick(0.7f);
                _system.Update();
                Assert.IsFalse(target.Has<EffectComponent<TestEffectMarker>>());

                var expiredFound = false;
                foreach (var e in receiver)
                {
                    if (e.Value.Expired)
                    {
                        expiredFound = true;
                    }
                }

                Assert.IsTrue(expiredFound);
                Assert.AreEqual(1, _handler.Removed.Count);
                Assert.IsTrue(_handler.Removed[0].Expired);
            }
            finally
            {
                World<TestEffectsWorld>.DeleteEventReceiver(ref receiver);
            }
        }

        [Test]
        public void Tick_WithPeriod_FiresHandlerEachPeriod()
        {
            var source = World<TestEffectsWorld>.NewEntity<Default>();
            var target = World<TestEffectsWorld>.NewEntity<Default>();
            EffectOperations.Apply<TestEffectsWorld, TestEffectMarker>(
                target.GID,
                source.GID,
                duration: 5f,
                period: 1f
            );

            Tick(1f);
            _system.Update();
            Assert.AreEqual(1, _handler.Ticks.Count);

            Tick(2f);
            _system.Update();
            Assert.AreEqual(3, _handler.Ticks.Count);
        }

        [Test]
        public void DelayedApply_ActivatesAfterDelayWithoutConsumingDuration()
        {
            var receiver = World<TestEffectsWorld>.RegisterEventReceiver<
                EffectAppliedEvent<TestEffectMarker>
            >();
            try
            {
                var source = World<TestEffectsWorld>.NewEntity<Default>();
                var target = World<TestEffectsWorld>.NewEntity<Default>();

                EffectOperations.Apply<TestEffectsWorld, TestEffectMarker>(
                    target.GID,
                    source.GID,
                    duration: 1f,
                    period: 0.5f,
                    delay: 2f);

                Assert.IsTrue(target.Has<PendingEffectComponent<TestEffectMarker>>());
                Assert.IsFalse(target.Has<EffectComponent<TestEffectMarker>>());
                Assert.IsFalse(
                    target.Has<World<TestEffectsWorld>.Multi<EffectSummaryComponent>>());
                Assert.AreEqual(0, _handler.Applied.Count);

                Tick(1.5f);
                _system.Update();
                Assert.IsTrue(target.Has<PendingEffectComponent<TestEffectMarker>>());

                Tick(0.5f);
                _system.Update();
                Assert.IsFalse(target.Has<PendingEffectComponent<TestEffectMarker>>());
                Assert.IsTrue(target.Has<EffectComponent<TestEffectMarker>>());
                Assert.AreEqual(
                    1f,
                    target.Read<EffectComponent<TestEffectMarker>>().TimeLeft,
                    0.0001f);
                Assert.AreEqual(1, _handler.Applied.Count);
                Assert.AreEqual(0, _handler.Ticks.Count);

                var appliedEvents = 0;
                foreach (var _ in receiver)
                {
                    appliedEvents++;
                }

                Assert.AreEqual(1, appliedEvents);

                Tick(0.5f);
                _system.Update();
                Assert.AreEqual(1, _handler.Ticks.Count);
                Assert.IsTrue(target.Has<EffectComponent<TestEffectMarker>>());
            }
            finally
            {
                World<TestEffectsWorld>.DeleteEventReceiver(ref receiver);
            }
        }

        [Test]
        public void ReapplyDuringDelay_AccumulatesStacksAndAppliesOnce()
        {
            var source = World<TestEffectsWorld>.NewEntity<Default>();
            var target = World<TestEffectsWorld>.NewEntity<Default>();

            EffectOperations.Apply<TestEffectsWorld, TestEffectMarker>(
                target.GID,
                source.GID,
                duration: 1f,
                delay: 2f);
            EffectOperations.Apply<TestEffectsWorld, TestEffectMarker>(
                target.GID,
                source.GID,
                duration: 3f,
                delay: 1f);

            Assert.AreEqual(
                2,
                target.Read<PendingEffectComponent<TestEffectMarker>>().Stacks);
            Tick(1f);
            _system.Update();

            Assert.AreEqual(1, _handler.Applied.Count);
            Assert.AreEqual(2, _handler.Applied[0].Stacks);
            Assert.AreEqual(
                3f,
                target.Read<EffectComponent<TestEffectMarker>>().TimeLeft,
                0.0001f);
        }

        [Test]
        public void PendingEffect_IsRemovedWhenSourceIsDestroyed()
        {
            var source = World<TestEffectsWorld>.NewEntity<Default>();
            var target = World<TestEffectsWorld>.NewEntity<Default>();
            EffectOperations.Apply<TestEffectsWorld, TestEffectMarker>(
                target.GID,
                source.GID,
                duration: 1f,
                delay: 10f);

            source.Destroy();

            Assert.IsFalse(target.Has<PendingEffectComponent<TestEffectMarker>>());
            Assert.IsFalse(target.Has<EffectComponent<TestEffectMarker>>());
            Assert.AreEqual(0, _handler.Applied.Count);
            Assert.AreEqual(0, _handler.Removed.Count);
        }

        [Test]
        public void SourceDestroyed_BackRefRemovesEffect_NotExpired_NoTickRequired()
        {
            var source = World<TestEffectsWorld>.NewEntity<Default>();
            var target = World<TestEffectsWorld>.NewEntity<Default>();
            EffectOperations.Apply<TestEffectsWorld, TestEffectMarker>(
                target.GID,
                source.GID,
                duration: 10f
            );

            source.Destroy();

            Assert.IsFalse(
                target.Has<EffectComponent<TestEffectMarker>>(),
                "Effect must be removed synchronously by the source-destroy hook, before any tick runs."
            );
            Assert.AreEqual(1, _handler.Removed.Count);
            Assert.IsFalse(_handler.Removed[0].Expired);
        }
    }
}
