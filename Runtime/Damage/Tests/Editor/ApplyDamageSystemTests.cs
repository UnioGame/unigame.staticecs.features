namespace UniGame.StaticEcs.Features.Tests
{
    using FFS.Libraries.StaticEcs;
    using NUnit.Framework;
    using UniGame.StaticEcs.Tests;

    [TestFixture]
    public sealed class ApplyDamageSystemTests
    {
        private FakeDamageRng _rng;
        private ApplyDamageSystem<TestDamageWorld> _system;
        private StaticEcsTestWorld<TestDamageWorld> _world;

        [SetUp]
        public void SetUp()
        {
            _world = new StaticEcsTestWorld<TestDamageWorld>();
            RegisterCharacteristicTypes();
            _rng = new FakeDamageRng();
            World<TestDamageWorld>.SetResource<IDamageRng>(_rng);

            new ModifierBackRefFeature<TestDamageWorld>().InstallResourcesAndRegisterTypesForTest(
                _world
            );
            new CharacteristicFeature<TestDamageWorld, HealthCharacteristic>().InstallResourcesAndRegisterTypesForTest(
                _world
            );
            new CharacteristicFeature<TestDamageWorld, ShieldCharacteristic>().InstallResourcesAndRegisterTypesForTest(
                _world
            );
            new CharacteristicFeature<TestDamageWorld, BlockChanceCharacteristic>().InstallResourcesAndRegisterTypesForTest(
                _world
            );
            new CharacteristicFeature<TestDamageWorld, DodgeChanceCharacteristic>().InstallResourcesAndRegisterTypesForTest(
                _world
            );
            new CharacteristicFeature<TestDamageWorld, ArmorResistCharacteristic>().InstallResourcesAndRegisterTypesForTest(
                _world
            );
            new CharacteristicFeature<
                TestDamageWorld,
                CriticalChanceCharacteristic
            >().InstallResourcesAndRegisterTypesForTest(_world);
            new CharacteristicFeature<
                TestDamageWorld,
                CriticalMultiplierCharacteristic
            >().InstallResourcesAndRegisterTypesForTest(_world);
            new DamageFeature<TestDamageWorld>().InstallResourcesAndRegisterTypesForTest(
                _world
            );

            _world.Initialize();

            _system = new ApplyDamageSystem<TestDamageWorld>();
            _system.Init();
        }

        [TearDown]
        public void TearDown()
        {
            _world?.TerminateLifeTime();
            if (World<TestDamageWorld>.Status == WorldStatus.Initialized)
            {
                _system.Destroy();
            }

            _world?.Dispose();
        }

        [Test]
        public void RaiseDamage_AppliesToHealth_NoFilters()
        {
            var source = World<TestDamageWorld>.NewEntity<Default>();
            var target = World<TestDamageWorld>.NewEntity<Default>();
            target.Set(CharacteristicComponent<HealthCharacteristic>.Create(100f, 0f, 100f));

            DamageOperations.RaiseDamage<TestDamageWorld>(source.GID, target.GID, 30f);
            _system.Update();

            Assert.AreEqual(
                70f,
                target.Read<CharacteristicComponent<HealthCharacteristic>>().Value
            );
            Assert.IsFalse(target.Has<DeathPendingTag>());
        }

        [Test]
        public void LethalHit_SetsDeathPendingTag()
        {
            var source = World<TestDamageWorld>.NewEntity<Default>();
            var target = World<TestDamageWorld>.NewEntity<Default>();
            target.Set(CharacteristicComponent<HealthCharacteristic>.Create(20f, 0f, 100f));

            DamageOperations.RaiseDamage<TestDamageWorld>(source.GID, target.GID, 50f);
            _system.Update();

            Assert.AreEqual(0f, target.Read<CharacteristicComponent<HealthCharacteristic>>().Value);
            Assert.IsTrue(target.Has<DeathPendingTag>());
        }

        private static void RegisterCharacteristicTypes()
        {
            var types = World<TestDamageWorld>.Types();
            CharacteristicTypeRegistration.Register<TestDamageWorld, HealthCharacteristic>(types);
            CharacteristicTypeRegistration.Register<TestDamageWorld, ShieldCharacteristic>(types);
            CharacteristicTypeRegistration.Register<TestDamageWorld, BlockChanceCharacteristic>(types);
            CharacteristicTypeRegistration.Register<TestDamageWorld, DodgeChanceCharacteristic>(types);
            CharacteristicTypeRegistration.Register<TestDamageWorld, ArmorResistCharacteristic>(types);
            CharacteristicTypeRegistration.Register<TestDamageWorld, CriticalChanceCharacteristic>(types);
            CharacteristicTypeRegistration.Register<TestDamageWorld, CriticalMultiplierCharacteristic>(types);
        }

        [Test]
        public void Overkill_ReportsRequestedAndAppliedAmounts()
        {
            var receiver = World<TestDamageWorld>.RegisterEventReceiver<DamageAppliedEvent>();
            var source = World<TestDamageWorld>.NewEntity<Default>();
            var target = World<TestDamageWorld>.NewEntity<Default>();
            target.Set(CharacteristicComponent<HealthCharacteristic>.Create(20f, 0f, 100f));

            DamageOperations.RaiseDamage<TestDamageWorld>(source.GID, target.GID, 50f);
            _system.Update();

            foreach (var damageEvent in receiver)
            {
                Assert.AreEqual(50f, damageEvent.Value.Amount);
                Assert.AreEqual(20f, damageEvent.Value.AppliedAmount);
            }

            World<TestDamageWorld>.DeleteEventReceiver(ref receiver);
        }

        [Test]
        public void RaiseHealing_IncreasesHealth()
        {
            var source = World<TestDamageWorld>.NewEntity<Default>();
            var target = World<TestDamageWorld>.NewEntity<Default>();
            target.Set(CharacteristicComponent<HealthCharacteristic>.Create(40f, 0f, 100f));

            DamageOperations.RaiseHealing<TestDamageWorld>(source.GID, target.GID, 25f);
            _system.Update();

            Assert.AreEqual(
                65f,
                target.Read<CharacteristicComponent<HealthCharacteristic>>().Value
            );
        }

        [Test]
        public void ClampedHealing_ReportsActualAppliedAmount()
        {
            var receiver = World<TestDamageWorld>.RegisterEventReceiver<DamageAppliedEvent>();
            var source = World<TestDamageWorld>.NewEntity<Default>();
            var target = World<TestDamageWorld>.NewEntity<Default>();
            target.Set(CharacteristicComponent<HealthCharacteristic>.Create(90f, 0f, 100f));

            DamageOperations.RaiseHealing<TestDamageWorld>(source.GID, target.GID, 25f);
            _system.Update();

            foreach (var damageEvent in receiver)
            {
                Assert.AreEqual(25f, damageEvent.Value.Amount);
                Assert.AreEqual(10f, damageEvent.Value.AppliedAmount);
            }

            World<TestDamageWorld>.DeleteEventReceiver(ref receiver);
        }

        [Test]
        public void DodgedDamage_DoesNotChangeHealth()
        {
            var source = World<TestDamageWorld>.NewEntity<Default>();
            var target = World<TestDamageWorld>.NewEntity<Default>();
            target.Set(CharacteristicComponent<HealthCharacteristic>.Create(100f, 0f, 100f));
            target.Set(CharacteristicComponent<DodgeChanceCharacteristic>.Create(1f, 0f, 1f));
            _rng.NextRoll = true;

            DamageOperations.RaiseDamage<TestDamageWorld>(source.GID, target.GID, 30f);
            _system.Update();

            Assert.AreEqual(
                100f,
                target.Read<CharacteristicComponent<HealthCharacteristic>>().Value
            );
        }
    }
}
