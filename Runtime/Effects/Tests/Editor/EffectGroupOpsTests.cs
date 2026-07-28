namespace UniGame.StaticEcs.Features.Tests
{
    using FFS.Libraries.StaticEcs;
    using NUnit.Framework;
    using UniGame.StaticEcs.Tests;
    using UniGame.StaticEcs.Time;

    [TestFixture]
    public sealed class EffectGroupOpsTests
    {
        private RecordingEffectHandler<TestEffectsWorld, TestEffectMarker> _handlerA;
        private RecordingEffectHandler<TestEffectsWorld, TestEffectMarkerB> _handlerB;
        private StaticEcsTestWorld<TestEffectsWorld> _world;

        [SetUp]
        public void SetUp()
        {
            _world = new StaticEcsTestWorld<TestEffectsWorld>();
            var types = _world.Types;
            EffectTypeRegistration.Register<TestEffectsWorld, TestEffectMarker>(types);
            EffectTypeRegistration.Register<TestEffectsWorld, TestEffectMarkerB>(types);
            _handlerA = new RecordingEffectHandler<TestEffectsWorld, TestEffectMarker>();
            _handlerB = new RecordingEffectHandler<TestEffectsWorld, TestEffectMarkerB>();

            new EffectsCoreFeature<TestEffectsWorld>().InstallResourcesAndRegisterTypesForTest(
                _world
            );
            new EcsTimeFeature<TestEffectsWorld>().InstallResourcesAndRegisterTypesForTest(
                _world
            );
            World<TestEffectsWorld>.SetResource<IEffectHandler<TestEffectsWorld, TestEffectMarker>>(_handlerA);
            var configA =
                new EffectConfig<TestEffectsWorld, TestEffectMarker>(
                    registerTickSystem: false);
            World<TestEffectsWorld>.SetResource<IEffectHandler<TestEffectsWorld, TestEffectMarkerB>>(_handlerB);
            var configB =
                new EffectConfig<TestEffectsWorld, TestEffectMarkerB>(
                    registerTickSystem: false);

            World<TestEffectsWorld>.SetResource(configA);
            World<TestEffectsWorld>.SetResource(configB);
            new EffectFeature<TestEffectsWorld, TestEffectMarker>()
                .InstallResourcesAndRegisterTypesForTest(_world);
            new EffectFeature<TestEffectsWorld, TestEffectMarkerB>()
                .InstallResourcesAndRegisterTypesForTest(_world);

            _world.Initialize();
        }

        [TearDown]
        public void TearDown()
        {
            _world?.Dispose();
        }

        [Test]
        public void RemoveAll_StripsEveryActiveEffect()
        {
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
        public void RemoveByMask_OnlyMatchesGivenBits()
        {
            var source = World<TestEffectsWorld>.NewEntity<Default>();
            var target = World<TestEffectsWorld>.NewEntity<Default>();
            EffectOperations.Apply<TestEffectsWorld, TestEffectMarker>(target.GID, source.GID, 5f);
            EffectOperations.Apply<TestEffectsWorld, TestEffectMarkerB>(target.GID, source.GID, 5f);

            var removed = EffectOperations.RemoveByMask<TestEffectsWorld>(
                target.GID,
                EffectFlag.Reserved1
            );

            Assert.AreEqual(1, removed);
            Assert.IsTrue(
                target.Has<EffectComponent<TestEffectMarker>>(),
                "Reserved0 effect must survive a Reserved1-only mask."
            );
            Assert.IsFalse(target.Has<EffectComponent<TestEffectMarkerB>>());
        }

        [Test]
        public void RemoveByMask_ZeroMask_NoOp()
        {
            var source = World<TestEffectsWorld>.NewEntity<Default>();
            var target = World<TestEffectsWorld>.NewEntity<Default>();
            EffectOperations.Apply<TestEffectsWorld, TestEffectMarker>(target.GID, source.GID, 5f);

            var removed = EffectOperations.RemoveByMask<TestEffectsWorld>(
                target.GID,
                EffectFlag.None
            );

            Assert.AreEqual(0, removed);
            Assert.IsTrue(target.Has<EffectComponent<TestEffectMarker>>());
        }

        [Test]
        public void RemoveByMask_NoEffects_ReturnsZero()
        {
            var target = World<TestEffectsWorld>.NewEntity<Default>();
            var removed = EffectOperations.RemoveByMask<TestEffectsWorld>(
                target.GID,
                EffectFlag.Reserved0
            );
            Assert.AreEqual(0, removed);
        }
    }
}
