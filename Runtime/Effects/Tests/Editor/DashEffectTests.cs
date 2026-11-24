namespace UniGame.StaticEcs.Features.Tests
{
    using FFS.Libraries.StaticEcs;
    using NUnit.Framework;
    using UniGame.StaticEcs.Tests;
    using UniGame.StaticEcs.Time;

    [TestFixture]
    public sealed class DashEffectTests
    {
        private EffectTickSystem<TestEffectsWorld, DashEffect> _tick;
        private StaticEcsTestWorld<TestEffectsWorld> _world;

        [SetUp]
        public void SetUp()
        {
            _world = new StaticEcsTestWorld<TestEffectsWorld>();
            EffectTypeRegistration.Register<TestEffectsWorld, DashEffect>(_world.Types);

            new EffectsCoreFeature<TestEffectsWorld>().InstallResourcesAndRegisterTypesForTest(_world);
            World<TestEffectsWorld>.SetResource(EcsTime.Default());
            new DashEffectFeature<TestEffectsWorld>().InstallResourcesAndRegisterTypesForTest(_world);

            _world.Initialize();
            _tick = new EffectTickSystem<TestEffectsWorld, DashEffect>();
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

        private static EffectApplyResult Dash(
            EntityGID target,
            EntityGID source,
            float distance,
            float duration,
            float directionX = 1f,
            float directionZ = 0f)
        {
            MovementEffectOperations.Set<TestEffectsWorld>(target, directionX, directionZ);
            var applyParams = new EffectApplyParams { Duration = duration, Magnitude = distance };
            return EffectOperations.Apply<TestEffectsWorld, DashEffect>(target, source, in applyParams);
        }

        [Test]
        public void Apply_WithDirection_CreatesDashFromDistanceAndDuration()
        {
            var target = World<TestEffectsWorld>.NewEntity<Default>();

            var result = Dash(target.GID, target.GID, 5f, 0.25f, 3f, 4f);

            Assert.AreEqual(EffectApplyResult.Applied, result);
            Assert.IsTrue(DashOperations.IsPending<TestEffectsWorld>(target.GID));
            Assert.IsFalse(MovementEffectOperations.Has<TestEffectsWorld>(target.GID));
            var dash = target.Read<DashComponent>();
            Assert.AreEqual(0.6f, dash.DirectionX, 1e-5f);
            Assert.AreEqual(0.8f, dash.DirectionZ, 1e-5f);
            Assert.AreEqual(5f, dash.Distance, 1e-5f);
            Assert.AreEqual(0.25f, dash.Duration, 1e-5f);
        }

        [Test]
        public void Apply_WithoutDirection_IsRejected()
        {
            var target = World<TestEffectsWorld>.NewEntity<Default>();

            var result = Dash(target.GID, target.GID, 5f, 0.25f, 0f, 0f);

            Assert.AreEqual(EffectApplyResult.Rejected, result);
            Assert.IsFalse(EffectOperations.Has<TestEffectsWorld, DashEffect>(target.GID));
            Assert.IsFalse(DashOperations.IsPending<TestEffectsWorld>(target.GID));
        }

        [Test]
        public void TryNormalize_ZeroDirection_IsRefused()
        {
            Assert.IsFalse(MovementEffectOperations.TryNormalize(0f, 0f, out _, out _));
            Assert.IsFalse(MovementEffectOperations.TryNormalize(float.NaN, 1f, out _, out _));
        }

        [Test]
        public void Apply_WhileDashing_IsRejectedAndKeepsTheRunningDash()
        {
            var target = World<TestEffectsWorld>.NewEntity<Default>();
            Dash(target.GID, target.GID, 5f, 1f);

            var result = Dash(target.GID, target.GID, 9f, 1f, 0f, 1f);

            Assert.AreEqual(EffectApplyResult.Rejected, result);
            Assert.AreEqual(5f, target.Read<DashComponent>().Distance, 1e-5f);
        }

        [Test]
        public void Expire_CancelsAPendingDashAndFreesTheEffect()
        {
            var target = World<TestEffectsWorld>.NewEntity<Default>();
            Dash(target.GID, target.GID, 5f, 1f);

            Tick(2f);
            _tick.Update();

            Assert.IsFalse(EffectOperations.Has<TestEffectsWorld, DashEffect>(target.GID));
            Assert.IsFalse(DashOperations.IsPending<TestEffectsWorld>(target.GID));
        }
    }
}
