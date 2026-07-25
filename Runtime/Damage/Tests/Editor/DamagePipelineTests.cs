namespace UniGame.StaticEcs.Features.Tests
{
    using FFS.Libraries.StaticEcs;
    using NUnit.Framework;
    using UniGame.StaticEcs.Tests;

    [TestFixture]
    public sealed class DamagePipelineTests
    {
        private FakeDamageRng _rng;
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

        [TearDown]
        public void TearDown()
        {
            _world?.Dispose();
        }

        [Test]
        public void DodgedDamage_ShortCircuitsBeforeShield()
        {
            var source = World<TestDamageWorld>.NewEntity<Default>();
            var target = World<TestDamageWorld>.NewEntity<Default>();
            target.Set(CharacteristicComponent<DodgeChanceCharacteristic>.Create(1f, 0f, 1f));
            target.Set(CharacteristicComponent<ShieldCharacteristic>.Create(50f, 0f, 100f));
            _rng.NextRoll = true;

            ref var chain = ref World<TestDamageWorld>.GetResource<
                DamageFilterChain<TestDamageWorld>
            >();
            var ctx = DamageContext.FromEvent(
                new IncomingDamageEvent
                {
                    Source = source.GID,
                    Target = target.GID,
                    Amount = 30f,
                    Type = DamageType.Physical,
                }
            );
            chain.Apply(ref ctx);

            Assert.IsTrue(ctx.Cancelled);
            Assert.AreEqual(DamageCancelReason.Dodged, ctx.CancelReason);
            Assert.AreEqual(
                50f,
                target.Read<CharacteristicComponent<ShieldCharacteristic>>().Value
            );
        }

        [Test]
        public void FullChain_ArmorThenCritThenShield_AppliesInOrder()
        {
            var source = World<TestDamageWorld>.NewEntity<Default>();
            var target = World<TestDamageWorld>.NewEntity<Default>();
            target.Set(CharacteristicComponent<ArmorResistCharacteristic>.Create(0.5f, 0f, 1f));
            target.Set(CharacteristicComponent<ShieldCharacteristic>.Create(40f, 0f, 100f));
            _rng.NextRoll = false;

            ref var chain = ref World<TestDamageWorld>.GetResource<
                DamageFilterChain<TestDamageWorld>
            >();
            var ctx = DamageContext.FromEvent(
                new IncomingDamageEvent
                {
                    Source = source.GID,
                    Target = target.GID,
                    Amount = 100f,
                    Type = DamageType.Physical,
                    ForceCritical = true,
                }
            );
            chain.Apply(ref ctx);

            Assert.IsFalse(ctx.Cancelled);
            Assert.IsTrue(ctx.IsCritical);
            Assert.AreEqual(60f, ctx.Amount, 0.0001f);
            Assert.AreEqual(40f, ctx.ShieldAbsorbed, 0.0001f);
            Assert.AreEqual(0f, target.Read<CharacteristicComponent<ShieldCharacteristic>>().Value);
        }

        [Test]
        public void HealingType_BypassesAllFilters()
        {
            var source = World<TestDamageWorld>.NewEntity<Default>();
            var target = World<TestDamageWorld>.NewEntity<Default>();
            target.Set<BlockableTag>();
            target.Set(CharacteristicComponent<DodgeChanceCharacteristic>.Create(1f, 0f, 1f));
            target.Set(CharacteristicComponent<BlockChanceCharacteristic>.Create(1f, 0f, 1f));
            target.Set(CharacteristicComponent<ArmorResistCharacteristic>.Create(0.9f, 0f, 1f));
            target.Set(CharacteristicComponent<ShieldCharacteristic>.Create(20f, 0f, 100f));
            _rng.NextRoll = true;

            ref var chain = ref World<TestDamageWorld>.GetResource<
                DamageFilterChain<TestDamageWorld>
            >();
            var ctx = DamageContext.FromEvent(
                new IncomingDamageEvent
                {
                    Source = source.GID,
                    Target = target.GID,
                    Amount = 50f,
                    Type = DamageType.Healing,
                }
            );
            chain.Apply(ref ctx);

            Assert.IsFalse(ctx.Cancelled);
            Assert.AreEqual(50f, ctx.Amount, 0.0001f);
            Assert.AreEqual(
                20f,
                target.Read<CharacteristicComponent<ShieldCharacteristic>>().Value
            );
        }
    }
}
