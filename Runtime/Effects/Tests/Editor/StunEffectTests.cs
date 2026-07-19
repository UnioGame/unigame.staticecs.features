using FFS.Libraries.StaticEcs;
using NUnit.Framework;
using UniGame.StaticEcs.Time;
 

namespace UniGame.StaticEcs.Features.Tests {
    [TestFixture]
    public sealed class StunEffectTests {
        private EffectTickSystem<TestEffectsWorld, StunEffect> _tick;

        [SetUp]
        public void SetUp() {
            World<TestEffectsWorld>.Create(WorldConfig.Default());

            new EcsTimeFeature<TestEffectsWorld>(registerFixed: false).RegisterTypes(World<TestEffectsWorld>.Types());
            new ModifierBackRefFeature<TestEffectsWorld>().RegisterTypes(World<TestEffectsWorld>.Types());
            new StunFeature<TestEffectsWorld>().RegisterTypes(World<TestEffectsWorld>.Types());
            new StunEffectFeature<TestEffectsWorld>(registerTickSystem: false).RegisterTypes(World<TestEffectsWorld>.Types());

            World<TestEffectsWorld>.Initialize();
            _tick = new EffectTickSystem<TestEffectsWorld, StunEffect>();
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
        public void Apply_ActivatesStun_AddsSource() {
            var source = World<TestEffectsWorld>.NewEntity<Default>();
            var target = World<TestEffectsWorld>.NewEntity<Default>();

            EffectOperations.Apply<TestEffectsWorld, StunEffect>(target.GID, source.GID, duration: 2f);

            Assert.IsTrue(StunOperations.IsActive<TestEffectsWorld>(target.GID));
            Assert.AreEqual(1, StunOperations.SourceCount<TestEffectsWorld>(target.GID));
        }

        [Test]
        public void Expire_RemovesStunSource() {
            var source = World<TestEffectsWorld>.NewEntity<Default>();
            var target = World<TestEffectsWorld>.NewEntity<Default>();
            EffectOperations.Apply<TestEffectsWorld, StunEffect>(target.GID, source.GID, duration: 1f);

            Tick(2f);
            _tick.Update();

            Assert.IsFalse(StunOperations.IsActive<TestEffectsWorld>(target.GID));
            Assert.AreEqual(0, StunOperations.SourceCount<TestEffectsWorld>(target.GID));
        }

        [Test]
        public void ManualSourceCoexistsWithEffectSource() {
            var source = World<TestEffectsWorld>.NewEntity<Default>();
            var target = World<TestEffectsWorld>.NewEntity<Default>();
            var manual = World<TestEffectsWorld>.NewEntity<Default>();

            StunOperations.AddSource<TestEffectsWorld>(target.GID, manual.GID);
            EffectOperations.Apply<TestEffectsWorld, StunEffect>(target.GID, source.GID, duration: 1f);

            Assert.AreEqual(2, StunOperations.SourceCount<TestEffectsWorld>(target.GID));

            Tick(2f);
            _tick.Update();

            Assert.IsTrue(StunOperations.IsActive<TestEffectsWorld>(target.GID));
            Assert.AreEqual(1, StunOperations.SourceCount<TestEffectsWorld>(target.GID));
        }
    }
}
