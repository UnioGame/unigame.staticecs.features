namespace UniGame.StaticEcs.Features.Tests
{
    using FFS.Libraries.StaticEcs;
    using NUnit.Framework;
    using UniGame.StaticEcs.Tests;
    using UniGame.StaticEcs.Time;

    [TestFixture]
    public sealed class HealOverTimeTests
    {
        private EffectTickSystem<TestEffectsWorld, HealOverTimeEffect> _tick;
        private ApplyDamageSystem<TestEffectsWorld> _apply;
        private FakeDamageRng _rng;
        private StaticEcsTestWorld<TestEffectsWorld> _world;

        [SetUp]
        public void SetUp()
        {
            _world = new StaticEcsTestWorld<TestEffectsWorld>();
            RegisterClosedTypes();
            _rng = new FakeDamageRng();
            World<TestEffectsWorld>.SetResource<IDamageRng>(_rng);

            new EffectsCoreFeature<TestEffectsWorld>().InstallResourcesAndRegisterTypesForTest(
                _world
            );
            new EcsTimeFeature<TestEffectsWorld>().InstallResourcesAndRegisterTypesForTest(
                _world
            );
            new ModifierBackRefFeature<TestEffectsWorld>().InstallResourcesAndRegisterTypesForTest(
                _world
            );
            new CharacteristicFeature<TestEffectsWorld, HealthCharacteristic>().InstallResourcesAndRegisterTypesForTest(
                _world
            );
            new CharacteristicFeature<TestEffectsWorld, ShieldCharacteristic>().InstallResourcesAndRegisterTypesForTest(
                _world
            );
            new CharacteristicFeature<TestEffectsWorld, BlockChanceCharacteristic>().InstallResourcesAndRegisterTypesForTest(
                _world
            );
            new CharacteristicFeature<TestEffectsWorld, DodgeChanceCharacteristic>().InstallResourcesAndRegisterTypesForTest(
                _world
            );
            new CharacteristicFeature<TestEffectsWorld, ArmorResistCharacteristic>().InstallResourcesAndRegisterTypesForTest(
                _world
            );
            new CharacteristicFeature<TestEffectsWorld, CriticalChanceCharacteristic>().InstallResourcesAndRegisterTypesForTest(_world);
            new CharacteristicFeature<TestEffectsWorld, CriticalMultiplierCharacteristic>().InstallResourcesAndRegisterTypesForTest(_world);
            new DamageFeature<TestEffectsWorld>().InstallResourcesAndRegisterTypesForTest(
                _world
            );
            new HealOverTimeFeature<TestEffectsWorld>().InstallResourcesAndRegisterTypesForTest(
                _world
            );

            _world.Initialize();

            _tick = new EffectTickSystem<TestEffectsWorld, HealOverTimeEffect>();
            _apply = new ApplyDamageSystem<TestEffectsWorld>();
            _apply.Init();
        }

        private static void RegisterClosedTypes()
        {
            var types = World<TestEffectsWorld>.Types();
            CharacteristicTypeRegistration.Register<TestEffectsWorld, HealthCharacteristic>(types);
            CharacteristicTypeRegistration.Register<TestEffectsWorld, ShieldCharacteristic>(types);
            CharacteristicTypeRegistration.Register<TestEffectsWorld, BlockChanceCharacteristic>(types);
            CharacteristicTypeRegistration.Register<TestEffectsWorld, DodgeChanceCharacteristic>(types);
            CharacteristicTypeRegistration.Register<TestEffectsWorld, ArmorResistCharacteristic>(types);
            CharacteristicTypeRegistration.Register<TestEffectsWorld, CriticalChanceCharacteristic>(types);
            CharacteristicTypeRegistration.Register<TestEffectsWorld, CriticalMultiplierCharacteristic>(types);
            EffectTypeRegistration.Register<TestEffectsWorld, HealOverTimeEffect>(types);
        }

        [TearDown]
        public void TearDown()
        {
            _world?.TerminateLifeTime();
            if (World<TestEffectsWorld>.Status == WorldStatus.Initialized)
                _apply.Destroy();

            _world?.Dispose();
        }

        private static void Tick(float dt)
        {
            ref var time = ref World<TestEffectsWorld>.GetResource<EcsTime>();
            time.DeltaTime = dt;
        }

        [Test]
        public void TickRaisesHealing_AppliedToHealth()
        {
            var source = World<TestEffectsWorld>.NewEntity<Default>();
            var target = World<TestEffectsWorld>.NewEntity<Default>();
            target.Set(CharacteristicComponent<HealthCharacteristic>.Create(50f, 0f, 100f));

            HealOverTimeOperations.Apply<TestEffectsWorld>(
                target.GID,
                source.GID,
                healPerTick: 5f,
                duration: 5f,
                period: 1f
            );

            Tick(1f);
            _tick.Update();
            _apply.Update();

            Assert.AreEqual(
                55f,
                target.Read<CharacteristicComponent<HealthCharacteristic>>().Value,
                0.0001f
            );

            Tick(2f);
            _tick.Update();
            _apply.Update();

            Assert.AreEqual(
                65f,
                target.Read<CharacteristicComponent<HealthCharacteristic>>().Value,
                0.0001f
            );
        }

        [Test]
        public void HealScalesByStacks()
        {
            var source = World<TestEffectsWorld>.NewEntity<Default>();
            var target = World<TestEffectsWorld>.NewEntity<Default>();
            target.Set(CharacteristicComponent<HealthCharacteristic>.Create(50f, 0f, 100f));

            HealOverTimeOperations.Apply<TestEffectsWorld>(
                target.GID,
                source.GID,
                healPerTick: 5f,
                duration: 5f,
                period: 1f
            );
            HealOverTimeOperations.Apply<TestEffectsWorld>(
                target.GID,
                source.GID,
                healPerTick: 5f,
                duration: 5f,
                period: 1f
            );

            Tick(1f);
            _tick.Update();
            _apply.Update();

            Assert.AreEqual(
                60f,
                target.Read<CharacteristicComponent<HealthCharacteristic>>().Value,
                0.0001f
            );
        }

        [Test]
        public void Expiry_StopsTicks()
        {
            var source = World<TestEffectsWorld>.NewEntity<Default>();
            var target = World<TestEffectsWorld>.NewEntity<Default>();
            target.Set(CharacteristicComponent<HealthCharacteristic>.Create(50f, 0f, 100f));

            HealOverTimeOperations.Apply<TestEffectsWorld>(
                target.GID,
                source.GID,
                healPerTick: 5f,
                duration: 1.5f,
                period: 1f
            );

            Tick(1f);
            _tick.Update();
            _apply.Update();

            Tick(1f);
            _tick.Update();
            _apply.Update();

            var afterExpiry = target.Read<CharacteristicComponent<HealthCharacteristic>>().Value;

            Tick(2f);
            _tick.Update();
            _apply.Update();

            Assert.AreEqual(
                afterExpiry,
                target.Read<CharacteristicComponent<HealthCharacteristic>>().Value,
                0.0001f
            );
        }
    }
}
