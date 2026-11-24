namespace UniGame.StaticEcs.Features.Tests
{
    using FFS.Libraries.StaticEcs;
    using NUnit.Framework;
    using UniGame.StaticEcs.Tests;
    using UniGame.StaticEcs.Time;

    [TestFixture]
    public sealed class EffectStackingTests
    {
        private RecordingEffectHandler<TestEffectsWorld, TestEffectMarker> _handler;
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
            World<TestEffectsWorld>.SetResource(EcsTime.Default());
            World<TestEffectsWorld>.SetResource<IEffectHandler<TestEffectsWorld, TestEffectMarker>>(_handler);
            var config = new EffectConfig<TestEffectsWorld, TestEffectMarker>(
                maxStacks: 3,
                refreshOnReapply: true,
                registerTickSystem: false);
            World<TestEffectsWorld>.SetResource(config);
            new EffectFeature<TestEffectsWorld, TestEffectMarker>()
                .InstallResourcesAndRegisterTypesForTest(_world);

            _world.Initialize();
        }

        [TearDown]
        public void TearDown()
        {
            _world?.Dispose();
        }

        [Test]
        public void Reapply_IncrementsStacks_UpToMax()
        {
            var source = World<TestEffectsWorld>.NewEntity<Default>();
            var target = World<TestEffectsWorld>.NewEntity<Default>();

            EffectOperations.Apply<TestEffectsWorld, TestEffectMarker>(
                target.GID,
                source.GID,
                duration: 5f
            );
            EffectOperations.Apply<TestEffectsWorld, TestEffectMarker>(
                target.GID,
                source.GID,
                duration: 5f
            );
            Assert.AreEqual(2, target.Read<EffectComponent<TestEffectMarker>>().Stacks);

            EffectOperations.Apply<TestEffectsWorld, TestEffectMarker>(
                target.GID,
                source.GID,
                duration: 5f
            );
            Assert.AreEqual(3, target.Read<EffectComponent<TestEffectMarker>>().Stacks);

            EffectOperations.Apply<TestEffectsWorld, TestEffectMarker>(
                target.GID,
                source.GID,
                duration: 5f
            );
            Assert.AreEqual(
                3,
                target.Read<EffectComponent<TestEffectMarker>>().Stacks,
                "Stacks must be capped by MaxStacks"
            );
        }

        [Test]
        public void Refresh_ResetsTimeLeftToMax()
        {
            var source = World<TestEffectsWorld>.NewEntity<Default>();
            var target = World<TestEffectsWorld>.NewEntity<Default>();

            EffectOperations.Apply<TestEffectsWorld, TestEffectMarker>(
                target.GID,
                source.GID,
                duration: 10f
            );

            ref var data = ref target.Ref<EffectComponent<TestEffectMarker>>();
            data.TimeLeft = 2f;

            EffectOperations.Apply<TestEffectsWorld, TestEffectMarker>(
                target.GID,
                source.GID,
                duration: 5f
            );
            Assert.AreEqual(5f, target.Read<EffectComponent<TestEffectMarker>>().TimeLeft, 0.0001f);

            EffectOperations.Apply<TestEffectsWorld, TestEffectMarker>(
                target.GID,
                source.GID,
                duration: 1f
            );
            Assert.AreEqual(5f, target.Read<EffectComponent<TestEffectMarker>>().TimeLeft, 0.0001f);
        }

        [Test]
        public void EveryApplication_RaisesOneAppliedEvent_WithBothStackCounts()
        {
            var receiver = World<TestEffectsWorld>.RegisterEventReceiver<EffectAppliedEvent<TestEffectMarker>>();
            try
            {
                var source = World<TestEffectsWorld>.NewEntity<Default>();
                var target = World<TestEffectsWorld>.NewEntity<Default>();

                EffectOperations.Apply<TestEffectsWorld, TestEffectMarker>(
                    target.GID,
                    source.GID,
                    duration: 5f
                );
                EffectOperations.Apply<TestEffectsWorld, TestEffectMarker>(
                    target.GID,
                    source.GID,
                    duration: 5f
                );

                var stacks = new System.Collections.Generic.List<(int Stacks, int PreviousStacks)>();
                foreach (var e in receiver)
                    stacks.Add((e.Value.Stacks, e.Value.PreviousStacks));

                Assert.AreEqual(2, stacks.Count);
                Assert.Contains((1, 0), stacks, "a new effect has no previous stacks");
                Assert.Contains((2, 1), stacks, "a reapplication reports both stack counts");
            }
            finally
            {
                World<TestEffectsWorld>.DeleteEventReceiver(ref receiver);
            }
        }
    }
}
