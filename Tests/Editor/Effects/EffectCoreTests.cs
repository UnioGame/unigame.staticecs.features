using FFS.Libraries.StaticEcs;
using NUnit.Framework;
using unigame.staticecs.Time;

namespace unigame.staticecs.features.Tests {
    [TestFixture]
    public sealed class EffectCoreTests {
        private RecordingEffectHandler<TestEffectsWorld, TestEffectMarker> _handler;
        private EffectTickSystem<TestEffectsWorld, TestEffectMarker> _system;

        [SetUp]
        public void SetUp() {
            World<TestEffectsWorld>.Create(WorldConfig.Default());
            _handler = new RecordingEffectHandler<TestEffectsWorld, TestEffectMarker>();

            new EcsTimeFeature<TestEffectsWorld>(registerFixed: false).RegisterTypes(World<TestEffectsWorld>.Types());
            new EffectFeature<TestEffectsWorld, TestEffectMarker>(_handler, maxStacks: 3, refreshOnReapply: true, registerTickSystem: false)
                .RegisterTypes(World<TestEffectsWorld>.Types());

            World<TestEffectsWorld>.Initialize();
            _system = new EffectTickSystem<TestEffectsWorld, TestEffectMarker>();
        }

        [TearDown]
        public void TearDown() {
            if (World<TestEffectsWorld>.Status != WorldStatus.NotCreated) {
                World<TestEffectsWorld>.Destroy();
            }
        }

        private static void Tick(float dt) {
            ref var time = ref World<TestEffectsWorld>.GetResource<EcsTime>();
            time.DeltaTime = dt;
        }

        [Test]
        public void Apply_AddsComponentAndRoster_RaisesAppliedEvent() {
            var receiver = World<TestEffectsWorld>.RegisterEventReceiver<EffectAppliedEvent<TestEffectMarker>>();
            try {
                var source = World<TestEffectsWorld>.NewEntity<Default>();
                var target = World<TestEffectsWorld>.NewEntity<Default>();

                Assert.IsTrue(EffectOperations.Apply<TestEffectsWorld, TestEffectMarker>(target.GID, source.GID, duration: 5f));

                Assert.IsTrue(target.Has<EffectComponent<TestEffectMarker>>());
                Assert.AreEqual(5f, target.Read<EffectComponent<TestEffectMarker>>().TimeLeft);
                Assert.AreEqual(1, target.Read<EffectComponent<TestEffectMarker>>().Stacks);

                Assert.IsTrue(target.Has<World<TestEffectsWorld>.Multi<EffectRosterEntry>>());
                var roster = target.Read<World<TestEffectsWorld>.Multi<EffectRosterEntry>>();
                Assert.AreEqual(1, roster.Length);
                Assert.AreEqual(1, roster[0].Stacks);

                var count = 0;
                foreach (var _ in receiver) {
                    count++;
                }

                Assert.AreEqual(1, count);
                Assert.AreEqual(1, _handler.Applied.Count);
                Assert.AreEqual(0, _handler.Applied[0].PreviousStacks);
            } finally {
                World<TestEffectsWorld>.DeleteEventReceiver(ref receiver);
            }
        }

        [Test]
        public void Remove_DeletesComponentAndRoster_RaisesRemovedEvent() {
            var receiver = World<TestEffectsWorld>.RegisterEventReceiver<EffectRemovedEvent<TestEffectMarker>>();
            try {
                var source = World<TestEffectsWorld>.NewEntity<Default>();
                var target = World<TestEffectsWorld>.NewEntity<Default>();
                EffectOperations.Apply<TestEffectsWorld, TestEffectMarker>(target.GID, source.GID, duration: 5f);

                Assert.IsTrue(EffectOperations.Remove<TestEffectsWorld, TestEffectMarker>(target.GID));
                Assert.IsFalse(target.Has<EffectComponent<TestEffectMarker>>());
                var roster = target.Read<World<TestEffectsWorld>.Multi<EffectRosterEntry>>();
                Assert.AreEqual(0, roster.Length);

                var found = false;
                foreach (var e in receiver) {
                    Assert.IsFalse(e.Value.Expired);
                    found = true;
                }

                Assert.IsTrue(found);
                Assert.AreEqual(1, _handler.Removed.Count);
                Assert.IsFalse(_handler.Removed[0].Expired);
            } finally {
                World<TestEffectsWorld>.DeleteEventReceiver(ref receiver);
            }
        }

        [Test]
        public void Tick_WithoutPeriod_DoesNotInvokeHandler_AndExpiresOnTimeOut() {
            var receiver = World<TestEffectsWorld>.RegisterEventReceiver<EffectRemovedEvent<TestEffectMarker>>();
            try {
                var source = World<TestEffectsWorld>.NewEntity<Default>();
                var target = World<TestEffectsWorld>.NewEntity<Default>();
                EffectOperations.Apply<TestEffectsWorld, TestEffectMarker>(target.GID, source.GID, duration: 1f);

                Tick(0.4f);
                _system.Update();
                Assert.IsTrue(target.Has<EffectComponent<TestEffectMarker>>());
                Assert.AreEqual(0, _handler.Ticks.Count);

                Tick(0.7f);
                _system.Update();
                Assert.IsFalse(target.Has<EffectComponent<TestEffectMarker>>());

                var expiredFound = false;
                foreach (var e in receiver) {
                    if (e.Value.Expired) {
                        expiredFound = true;
                    }
                }

                Assert.IsTrue(expiredFound);
                Assert.AreEqual(1, _handler.Removed.Count);
                Assert.IsTrue(_handler.Removed[0].Expired);
            } finally {
                World<TestEffectsWorld>.DeleteEventReceiver(ref receiver);
            }
        }

        [Test]
        public void Tick_WithPeriod_FiresHandlerEachPeriod() {
            var source = World<TestEffectsWorld>.NewEntity<Default>();
            var target = World<TestEffectsWorld>.NewEntity<Default>();
            EffectOperations.Apply<TestEffectsWorld, TestEffectMarker>(target.GID, source.GID, duration: 5f, period: 1f);

            Tick(1f);
            _system.Update();
            Assert.AreEqual(1, _handler.Ticks.Count);

            Tick(2f);
            _system.Update();
            Assert.AreEqual(3, _handler.Ticks.Count);
        }

        [Test]
        public void SourceDestroyed_BackRefRemovesEffect_NotExpired_NoTickRequired() {
            var source = World<TestEffectsWorld>.NewEntity<Default>();
            var target = World<TestEffectsWorld>.NewEntity<Default>();
            EffectOperations.Apply<TestEffectsWorld, TestEffectMarker>(target.GID, source.GID, duration: 10f);

            source.Destroy();

            Assert.IsFalse(target.Has<EffectComponent<TestEffectMarker>>(),
                "Effect must be removed synchronously by the source-destroy hook, before any tick runs.");
            Assert.AreEqual(1, _handler.Removed.Count);
            Assert.IsFalse(_handler.Removed[0].Expired);
        }
    }
}
