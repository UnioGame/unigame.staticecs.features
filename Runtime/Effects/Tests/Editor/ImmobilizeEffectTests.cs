namespace UniGame.StaticEcs.Features.Tests
{
    using FFS.Libraries.StaticEcs;
    using NUnit.Framework;
    using UniGame.StaticEcs.Tests;
    using UniGame.StaticEcs.Time;

    [TestFixture]
    public sealed class ImmobilizeEffectTests
    {
        private EffectTickSystem<TestEffectsWorld, ImmobilizeEffect> _tick;
        private StaticEcsTestWorld<TestEffectsWorld> _world;

        [SetUp]
        public void SetUp()
        {
            _world = new StaticEcsTestWorld<TestEffectsWorld>();
            EffectTypeRegistration.Register<TestEffectsWorld, ImmobilizeEffect>(
                _world.Types);

            new EffectsCoreFeature<TestEffectsWorld>().InstallResourcesAndRegisterTypesForTest(
                _world
            );
            World<TestEffectsWorld>.SetResource(EcsTime.Default());
            new ModifierBackRefFeature<TestEffectsWorld>().InstallResourcesAndRegisterTypesForTest(
                _world
            );
            new ImmobilizeFeature<TestEffectsWorld>()
                .InstallResourcesAndRegisterTypesForTest(_world);
            new ImmobilizeEffectFeature<TestEffectsWorld>().InstallResourcesAndRegisterTypesForTest(
                _world
            );

            _world.Initialize();
            _tick = new EffectTickSystem<TestEffectsWorld, ImmobilizeEffect>();
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
        public void Apply_ActivatesImmobilize_AddsSource()
        {
            var source = World<TestEffectsWorld>.NewEntity<Default>();
            var target = World<TestEffectsWorld>.NewEntity<Default>();

            EffectOperations.Apply<TestEffectsWorld, ImmobilizeEffect>(
                target.GID,
                source.GID,
                duration: 2f
            );

            Assert.IsTrue(ImmobilizeOperations.IsActive<TestEffectsWorld>(target.GID));
            Assert.AreEqual(1, ImmobilizeOperations.SourceCount<TestEffectsWorld>(target.GID));
        }

        [Test]
        public void Expire_RemovesImmobilizeSource()
        {
            var source = World<TestEffectsWorld>.NewEntity<Default>();
            var target = World<TestEffectsWorld>.NewEntity<Default>();
            EffectOperations.Apply<TestEffectsWorld, ImmobilizeEffect>(
                target.GID,
                source.GID,
                duration: 1f
            );

            Tick(2f);
            _tick.Update();

            Assert.IsFalse(ImmobilizeOperations.IsActive<TestEffectsWorld>(target.GID));
            Assert.AreEqual(0, ImmobilizeOperations.SourceCount<TestEffectsWorld>(target.GID));
        }

        [Test]
        public void ManualSourceCoexistsWithEffectSource()
        {
            var source = World<TestEffectsWorld>.NewEntity<Default>();
            var target = World<TestEffectsWorld>.NewEntity<Default>();
            var manual = World<TestEffectsWorld>.NewEntity<Default>();

            ImmobilizeOperations.AddSource<TestEffectsWorld>(target.GID, manual.GID);
            EffectOperations.Apply<TestEffectsWorld, ImmobilizeEffect>(
                target.GID,
                source.GID,
                duration: 1f
            );

            Assert.AreEqual(2, ImmobilizeOperations.SourceCount<TestEffectsWorld>(target.GID));

            Tick(2f);
            _tick.Update();

            Assert.IsTrue(ImmobilizeOperations.IsActive<TestEffectsWorld>(target.GID));
            Assert.AreEqual(1, ImmobilizeOperations.SourceCount<TestEffectsWorld>(target.GID));
        }

        [Test]
        public void ReapplyFromAnotherSource_ExpiryLeavesNoSourceBehind()
        {
            var first = World<TestEffectsWorld>.NewEntity<Default>();
            var second = World<TestEffectsWorld>.NewEntity<Default>();
            var target = World<TestEffectsWorld>.NewEntity<Default>();
            EffectOperations.Apply<TestEffectsWorld, ImmobilizeEffect>(
                target.GID,
                first.GID,
                duration: 1f
            );
            EffectOperations.Apply<TestEffectsWorld, ImmobilizeEffect>(
                target.GID,
                second.GID,
                duration: 1f
            );

            Assert.AreEqual(1, ImmobilizeOperations.SourceCount<TestEffectsWorld>(target.GID));

            Tick(2f);
            _tick.Update();

            Assert.IsFalse(ImmobilizeOperations.IsActive<TestEffectsWorld>(target.GID));
            Assert.AreEqual(0, ImmobilizeOperations.SourceCount<TestEffectsWorld>(target.GID));
        }
    }
}
