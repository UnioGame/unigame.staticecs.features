namespace UniGame.StaticEcs.Features.Tests
{
    using FFS.Libraries.StaticEcs;
    using NUnit.Framework;
    using UniGame.StaticEcs.Tests;
    using UniGame.StaticEcs.Time;

    [TestFixture]
    public sealed class StunEffectTests
    {
        private EffectTickSystem<TestEffectsWorld, StunEffect> _tick;
        private StaticEcsTestWorld<TestEffectsWorld> _world;

        [SetUp]
        public void SetUp()
        {
            _world = new StaticEcsTestWorld<TestEffectsWorld>();
            EffectTypeRegistration.Register<TestEffectsWorld, StunEffect>(
                _world.Types);

            new EffectsCoreFeature<TestEffectsWorld>().InstallResourcesAndRegisterTypesForTest(
                _world
            );
            new EcsTimeFeature<TestEffectsWorld>().InstallResourcesAndRegisterTypesForTest(
                _world
            );
            new ModifierBackRefFeature<TestEffectsWorld>().InstallResourcesAndRegisterTypesForTest(
                _world
            );
            new StunFeature<TestEffectsWorld>()
                .InstallResourcesAndRegisterTypesForTest(_world);
            new StunEffectFeature<TestEffectsWorld>().InstallResourcesAndRegisterTypesForTest(
                _world
            );

            _world.Initialize();
            _tick = new EffectTickSystem<TestEffectsWorld, StunEffect>();
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
        public void Apply_ActivatesStun_AddsSource()
        {
            var source = World<TestEffectsWorld>.NewEntity<Default>();
            var target = World<TestEffectsWorld>.NewEntity<Default>();

            EffectOperations.Apply<TestEffectsWorld, StunEffect>(
                target.GID,
                source.GID,
                duration: 2f
            );

            Assert.IsTrue(StunOperations.IsActive<TestEffectsWorld>(target.GID));
            Assert.AreEqual(1, StunOperations.SourceCount<TestEffectsWorld>(target.GID));
        }

        [Test]
        public void Expire_RemovesStunSource()
        {
            var source = World<TestEffectsWorld>.NewEntity<Default>();
            var target = World<TestEffectsWorld>.NewEntity<Default>();
            EffectOperations.Apply<TestEffectsWorld, StunEffect>(
                target.GID,
                source.GID,
                duration: 1f
            );

            Tick(2f);
            _tick.Update();

            Assert.IsFalse(StunOperations.IsActive<TestEffectsWorld>(target.GID));
            Assert.AreEqual(0, StunOperations.SourceCount<TestEffectsWorld>(target.GID));
        }

        [Test]
        public void ManualSourceCoexistsWithEffectSource()
        {
            var source = World<TestEffectsWorld>.NewEntity<Default>();
            var target = World<TestEffectsWorld>.NewEntity<Default>();
            var manual = World<TestEffectsWorld>.NewEntity<Default>();

            StunOperations.AddSource<TestEffectsWorld>(target.GID, manual.GID);
            EffectOperations.Apply<TestEffectsWorld, StunEffect>(
                target.GID,
                source.GID,
                duration: 1f
            );

            Assert.AreEqual(2, StunOperations.SourceCount<TestEffectsWorld>(target.GID));

            Tick(2f);
            _tick.Update();

            Assert.IsTrue(StunOperations.IsActive<TestEffectsWorld>(target.GID));
            Assert.AreEqual(1, StunOperations.SourceCount<TestEffectsWorld>(target.GID));
        }
    }
}
