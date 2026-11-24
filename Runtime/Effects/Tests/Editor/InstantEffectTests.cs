namespace UniGame.StaticEcs.Features.Tests
{
    using FFS.Libraries.StaticEcs;
    using NUnit.Framework;
    using UniGame.StaticEcs.Tests;
    using UniGame.StaticEcs.Time;

    [TestFixture]
    public sealed class InstantEffectTests
    {
        private RecordingEffectHandler<TestEffectsWorld, TestEffectMarker> _handler;
        private StaticEcsTestWorld<TestEffectsWorld> _world;

        [SetUp]
        public void SetUp()
        {
            _world = new StaticEcsTestWorld<TestEffectsWorld>();
            EffectTypeRegistration.Register<TestEffectsWorld, TestEffectMarker>(_world.Types);
            _handler = new RecordingEffectHandler<TestEffectsWorld, TestEffectMarker>();

            new EffectsCoreFeature<TestEffectsWorld>().InstallResourcesAndRegisterTypesForTest(_world);
            World<TestEffectsWorld>.SetResource(EcsTime.Default());
            World<TestEffectsWorld>.SetResource<IEffectHandler<TestEffectsWorld, TestEffectMarker>>(_handler);
            World<TestEffectsWorld>.SetResource(EffectConfig<TestEffectsWorld, TestEffectMarker>.CreateInstant());
            new EffectFeature<TestEffectsWorld, TestEffectMarker>()
                .InstallResourcesAndRegisterTypesForTest(_world);
            _world.Initialize();
        }

        [TearDown]
        public void TearDown()
        {
            _world?.Dispose();
            _world = null;
        }

        [Test]
        public void Apply_InstantEffect_CallsHandlerOnceWithMagnitudeAndLeavesNoState()
        {
            var source = World<TestEffectsWorld>.NewEntity<Default>();
            var target = World<TestEffectsWorld>.NewEntity<Default>();

            var result = EffectOperations.Apply<TestEffectsWorld, TestEffectMarker>(
                target.GID,
                source.GID,
                new EffectApplyParams { Magnitude = 12f });

            Assert.AreEqual(EffectApplyResult.Applied, result);
            Assert.AreEqual(1, _handler.Applied.Count);
            Assert.AreEqual(target.GID, _handler.Applied[0].Target);
            Assert.AreEqual(source.GID, _handler.Applied[0].Source);
            Assert.AreEqual(0, _handler.Applied[0].PreviousStacks);
            Assert.AreEqual(12f, _handler.Magnitudes[0]);
            Assert.IsFalse(target.Has<EffectComponent<TestEffectMarker>>());
            Assert.IsFalse(target.Has<World<TestEffectsWorld>.Multi<EffectSummaryComponent>>());
        }

        [Test]
        public void Apply_InstantEffectTwice_CallsHandlerEveryTimeWithoutStacking()
        {
            var source = World<TestEffectsWorld>.NewEntity<Default>();
            var target = World<TestEffectsWorld>.NewEntity<Default>();

            EffectOperations.Apply<TestEffectsWorld, TestEffectMarker>(
                target.GID, source.GID, new EffectApplyParams { Magnitude = 1f });
            var second = EffectOperations.Apply<TestEffectsWorld, TestEffectMarker>(
                target.GID, source.GID, new EffectApplyParams { Magnitude = 2f });

            Assert.AreEqual(EffectApplyResult.Applied, second);
            Assert.AreEqual(2, _handler.Applied.Count);
            Assert.AreEqual(1, _handler.Applied[1].Stacks);
            Assert.AreEqual(2f, _handler.Magnitudes[1]);
        }

        [Test]
        public void Apply_HandlerRefuses_ReturnsRejectedWithoutApplying()
        {
            var source = World<TestEffectsWorld>.NewEntity<Default>();
            var target = World<TestEffectsWorld>.NewEntity<Default>();
            _handler.RejectApply = true;

            var result = EffectOperations.Apply<TestEffectsWorld, TestEffectMarker>(
                target.GID, source.GID, new EffectApplyParams { Magnitude = 5f });

            Assert.AreEqual(EffectApplyResult.Rejected, result);
            Assert.AreEqual(0, _handler.Applied.Count);
        }

        [Test]
        public void Apply_InstantEffect_IsAppliedAndThenRemovedAsExpired()
        {
            var applied = World<TestEffectsWorld>.RegisterEventReceiver<EffectAppliedEvent<TestEffectMarker>>();
            var removed = World<TestEffectsWorld>.RegisterEventReceiver<EffectRemovedEvent<TestEffectMarker>>();
            try
            {
                var source = World<TestEffectsWorld>.NewEntity<Default>();
                var target = World<TestEffectsWorld>.NewEntity<Default>();

                EffectOperations.Apply<TestEffectsWorld, TestEffectMarker>(
                    target.GID, source.GID, new EffectApplyParams { Magnitude = 3f });

                var removedCount = 0;
                var removedExpired = true;
                foreach (var e in removed)
                {
                    removedCount++;
                    removedExpired &= e.Value.Expired;
                }

                Assert.AreEqual(1, Count(applied), "one applied event");
                Assert.AreEqual(1, removedCount, "one removed event");
                Assert.IsTrue(removedExpired);
                Assert.AreEqual(1, _handler.Removed.Count);
                Assert.AreEqual(target.GID, _handler.Removed[0].Target);
                Assert.AreEqual(1, _handler.Removed[0].Stacks);
                Assert.IsTrue(_handler.Removed[0].Expired);
            }
            finally
            {
                World<TestEffectsWorld>.DeleteEventReceiver(ref applied);
                World<TestEffectsWorld>.DeleteEventReceiver(ref removed);
            }
        }

        [Test]
        public void Apply_InstantEffect_LeavesNoBackReferenceOnTheSource()
        {
            var source = World<TestEffectsWorld>.NewEntity<Default>();
            var target = World<TestEffectsWorld>.NewEntity<Default>();

            EffectOperations.Apply<TestEffectsWorld, TestEffectMarker>(
                target.GID, source.GID, new EffectApplyParams { Magnitude = 3f });

            Assert.IsFalse(source.Has<World<TestEffectsWorld>.Multi<EffectTargetComponent>>());
        }

        [Test]
        public void Apply_InstantEffectFromADestroyedSource_StillApplies()
        {
            var source = World<TestEffectsWorld>.NewEntity<Default>();
            var target = World<TestEffectsWorld>.NewEntity<Default>();
            var sourceGid = source.GID;
            source.Destroy();

            var result = EffectOperations.Apply<TestEffectsWorld, TestEffectMarker>(
                target.GID, sourceGid, new EffectApplyParams { Magnitude = 3f });

            Assert.AreEqual(EffectApplyResult.Applied, result);
            Assert.AreEqual(1, _handler.Applied.Count);
            Assert.AreEqual(1, _handler.Removed.Count);
        }

        [Test]
        public void Apply_InstantEffect_SeesNoStacksInCanApply()
        {
            var source = World<TestEffectsWorld>.NewEntity<Default>();
            var target = World<TestEffectsWorld>.NewEntity<Default>();

            EffectOperations.Apply<TestEffectsWorld, TestEffectMarker>(
                target.GID, source.GID, new EffectApplyParams { Magnitude = 1f });
            EffectOperations.Apply<TestEffectsWorld, TestEffectMarker>(
                target.GID, source.GID, new EffectApplyParams { Magnitude = 1f });

            Assert.AreEqual((0, 0), _handler.Checked[0]);
            Assert.AreEqual((0, 0), _handler.Checked[1]);
            Assert.AreEqual((1, 0), (_handler.Applied[1].Stacks, _handler.Applied[1].PreviousStacks));
        }

        private static int Count<TEvent>(EventReceiver<TestEffectsWorld, TEvent> receiver)
            where TEvent : struct, IEvent
        {
            var count = 0;
            foreach (var _ in receiver)
                count++;

            return count;
        }

        [Test]
        public void Apply_DestroyedTarget_Fails()
        {
            var source = World<TestEffectsWorld>.NewEntity<Default>();
            var target = World<TestEffectsWorld>.NewEntity<Default>();
            var gid = target.GID;
            target.Destroy();

            var result = EffectOperations.Apply<TestEffectsWorld, TestEffectMarker>(
                gid, source.GID, new EffectApplyParams { Magnitude = 5f });

            Assert.AreEqual(EffectApplyResult.Failed, result);
            Assert.AreEqual(0, _handler.Applied.Count);
        }
    }
}
