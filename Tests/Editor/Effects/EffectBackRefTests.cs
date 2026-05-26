using FFS.Libraries.StaticEcs;
using NUnit.Framework;
using unigame.staticecs.Time;

namespace unigame.staticecs.features.Tests {
    [TestFixture]
    public sealed class EffectBackRefTests {
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
        public void Apply_AddsBackRefOnSource_WithCorrectMask() {
            var source = World<TestEffectsWorld>.NewEntity<Default>();
            var target = World<TestEffectsWorld>.NewEntity<Default>();

            EffectOperations.Apply<TestEffectsWorld, TestEffectMarker>(target.GID, source.GID, 5f);

            Assert.IsTrue(source.Has<EffectSourceTracker>());
            Assert.IsTrue(source.Has<World<TestEffectsWorld>.Multi<EffectBackRef>>());
            ref var refs = ref source.Ref<World<TestEffectsWorld>.Multi<EffectBackRef>>();
            Assert.AreEqual(1, refs.Length);
            Assert.AreEqual(EffectFlag.Reserved0, refs[0].Mask);
        }

        [Test]
        public void TwoEffectsFromSameSource_ShareBackRefEntry_WithOredMask() {
            var source = World<TestEffectsWorld>.NewEntity<Default>();
            var target = World<TestEffectsWorld>.NewEntity<Default>();

            EffectOperations.Apply<TestEffectsWorld, TestEffectMarker>(target.GID, source.GID, 5f);
            EffectOperations.Apply<TestEffectsWorld, TestEffectMarkerB>(target.GID, source.GID, 5f);

            ref var refs = ref source.Ref<World<TestEffectsWorld>.Multi<EffectBackRef>>();
            Assert.AreEqual(1, refs.Length);
            Assert.AreEqual(EffectFlag.Reserved0 | EffectFlag.Reserved1, refs[0].Mask);
        }

        [Test]
        public void DestroyingSource_RemovesAllItsEffects_FromAllTargets() {
            var source = World<TestEffectsWorld>.NewEntity<Default>();
            var targetA = World<TestEffectsWorld>.NewEntity<Default>();
            var targetB = World<TestEffectsWorld>.NewEntity<Default>();

            EffectOperations.Apply<TestEffectsWorld, TestEffectMarker>(targetA.GID, source.GID, 10f);
            EffectOperations.Apply<TestEffectsWorld, TestEffectMarkerB>(targetA.GID, source.GID, 10f);
            EffectOperations.Apply<TestEffectsWorld, TestEffectMarker>(targetB.GID, source.GID, 10f);

            source.Destroy();

            Assert.IsFalse(targetA.Has<EffectComponent<TestEffectMarker>>());
            Assert.IsFalse(targetA.Has<EffectComponent<TestEffectMarkerB>>());
            Assert.IsFalse(targetB.Has<EffectComponent<TestEffectMarker>>());

            Assert.AreEqual(2, _handlerA.Removed.Count);
            Assert.AreEqual(1, _handlerB.Removed.Count);
            Assert.IsFalse(_handlerA.Removed[0].Expired);
            Assert.IsFalse(_handlerB.Removed[0].Expired);
        }

        [Test]
        public void DestroyingSource_OnlyAffectsItsOwnEffects() {
            var sourceA = World<TestEffectsWorld>.NewEntity<Default>();
            var sourceB = World<TestEffectsWorld>.NewEntity<Default>();
            var target = World<TestEffectsWorld>.NewEntity<Default>();

            EffectOperations.Apply<TestEffectsWorld, TestEffectMarker>(target.GID, sourceA.GID, 10f);
            // Re-apply from a different source overwrites .Source per single-source contract.
            EffectOperations.Apply<TestEffectsWorld, TestEffectMarker>(target.GID, sourceB.GID, 10f);

            sourceA.Destroy();

            // Effect now sourced by B, so A's destroy must not strip it.
            Assert.IsTrue(target.Has<EffectComponent<TestEffectMarker>>(),
                "Effect re-sourced to a still-living entity must survive the original source destroy.");
        }

        [Test]
        public void Remove_ClearsBackRefEntry_OnSource() {
            var source = World<TestEffectsWorld>.NewEntity<Default>();
            var target = World<TestEffectsWorld>.NewEntity<Default>();

            EffectOperations.Apply<TestEffectsWorld, TestEffectMarker>(target.GID, source.GID, 5f);
            Assert.IsTrue(EffectOperations.Remove<TestEffectsWorld, TestEffectMarker>(target.GID));

            ref var refs = ref source.Ref<World<TestEffectsWorld>.Multi<EffectBackRef>>();
            Assert.AreEqual(0, refs.Length);
        }
    }
}
