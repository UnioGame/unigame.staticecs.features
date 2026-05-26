using FFS.Libraries.StaticEcs;
using NUnit.Framework;
using unigame.staticecs.Time;

namespace unigame.staticecs.features.Tests {
    [TestFixture]
    public sealed class EffectGroupOpsTests {
        private RecordingEffectHandler<TestEffectsWorld, TestEffectMarker> _handlerA;
        private RecordingEffectHandler<TestEffectsWorld, TestEffectMarkerB> _handlerB;

        [SetUp]
        public void SetUp() {
            World<TestEffectsWorld>.Create(WorldConfig.Default());
            _handlerA = new RecordingEffectHandler<TestEffectsWorld, TestEffectMarker>();
            _handlerB = new RecordingEffectHandler<TestEffectsWorld, TestEffectMarkerB>();

            new EcsTimeFeature<TestEffectsWorld>(registerFixed: false).RegisterTypes(World<TestEffectsWorld>.Types());
            new EffectFeature<TestEffectsWorld, TestEffectMarker>(_handlerA, registerTickSystem: false)
                .RegisterTypes(World<TestEffectsWorld>.Types());
            new EffectFeature<TestEffectsWorld, TestEffectMarkerB>(_handlerB, registerTickSystem: false)
                .RegisterTypes(World<TestEffectsWorld>.Types());

            World<TestEffectsWorld>.Initialize();
        }

        [TearDown]
        public void TearDown() {
            if (World<TestEffectsWorld>.Status != WorldStatus.NotCreated) {
                World<TestEffectsWorld>.Destroy();
            }
        }

        [Test]
        public void RemoveAll_StripsEveryActiveEffect() {
            var source = World<TestEffectsWorld>.NewEntity<Default>();
            var target = World<TestEffectsWorld>.NewEntity<Default>();
            EffectOperations.Apply<TestEffectsWorld, TestEffectMarker>(target.GID, source.GID, 5f);
            EffectOperations.Apply<TestEffectsWorld, TestEffectMarkerB>(target.GID, source.GID, 5f);

            var removed = EffectOperations.RemoveAll<TestEffectsWorld>(target.GID);

            Assert.AreEqual(2, removed);
            Assert.IsFalse(target.Has<EffectComponent<TestEffectMarker>>());
            Assert.IsFalse(target.Has<EffectComponent<TestEffectMarkerB>>());
        }

        [Test]
        public void RemoveByMask_OnlyMatchesGivenBits() {
            var source = World<TestEffectsWorld>.NewEntity<Default>();
            var target = World<TestEffectsWorld>.NewEntity<Default>();
            EffectOperations.Apply<TestEffectsWorld, TestEffectMarker>(target.GID, source.GID, 5f);
            EffectOperations.Apply<TestEffectsWorld, TestEffectMarkerB>(target.GID, source.GID, 5f);

            var removed = EffectOperations.RemoveByMask<TestEffectsWorld>(target.GID, EffectFlag.Reserved1);

            Assert.AreEqual(1, removed);
            Assert.IsTrue(target.Has<EffectComponent<TestEffectMarker>>(),
                "Reserved0 effect must survive a Reserved1-only mask.");
            Assert.IsFalse(target.Has<EffectComponent<TestEffectMarkerB>>());
        }

        [Test]
        public void RemoveByMask_ZeroMask_NoOp() {
            var source = World<TestEffectsWorld>.NewEntity<Default>();
            var target = World<TestEffectsWorld>.NewEntity<Default>();
            EffectOperations.Apply<TestEffectsWorld, TestEffectMarker>(target.GID, source.GID, 5f);

            var removed = EffectOperations.RemoveByMask<TestEffectsWorld>(target.GID, EffectFlag.None);

            Assert.AreEqual(0, removed);
            Assert.IsTrue(target.Has<EffectComponent<TestEffectMarker>>());
        }

        [Test]
        public void RemoveByMask_NoEffects_ReturnsZero() {
            var target = World<TestEffectsWorld>.NewEntity<Default>();
            var removed = EffectOperations.RemoveByMask<TestEffectsWorld>(target.GID, EffectFlag.Reserved0);
            Assert.AreEqual(0, removed);
        }
    }
}
