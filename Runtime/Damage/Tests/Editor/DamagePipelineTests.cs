namespace UniGame.StaticEcs.Features.Tests
{
    using FFS.Libraries.StaticEcs;
    using NUnit.Framework;

    [TestFixture]
    public sealed class DamagePipelineTests
    {
        private FakeDamageRng _rng;

        [SetUp]
        public void SetUp()
        {
            World<TestDamageWorld>.Create(WorldConfig.Default());
            _rng = new FakeDamageRng();
            World<TestDamageWorld>.SetResource<IDamageRng>(_rng);

            new ModifierBackRefFeature<TestDamageWorld>().RegisterTypes(
                World<TestDamageWorld>.Types()
            );
            new CharacteristicFeature<TestDamageWorld, HealthCharacteristic>().RegisterTypes(
                World<TestDamageWorld>.Types()
            );
            new CharacteristicFeature<TestDamageWorld, ShieldCharacteristic>().RegisterTypes(
                World<TestDamageWorld>.Types()
            );
            new CharacteristicFeature<TestDamageWorld, BlockChanceCharacteristic>().RegisterTypes(
                World<TestDamageWorld>.Types()
            );
            new CharacteristicFeature<TestDamageWorld, DodgeChanceCharacteristic>().RegisterTypes(
                World<TestDamageWorld>.Types()
            );
            new CharacteristicFeature<TestDamageWorld, ArmorResistCharacteristic>().RegisterTypes(
                World<TestDamageWorld>.Types()
            );
            new CharacteristicFeature<
                TestDamageWorld,
                CriticalChanceCharacteristic
            >().RegisterTypes(World<TestDamageWorld>.Types());
            new CharacteristicFeature<
                TestDamageWorld,
                CriticalMultiplierCharacteristic
            >().RegisterTypes(World<TestDamageWorld>.Types());
            new DamageFeature<TestDamageWorld>(registerApplySystem: false).RegisterTypes(
                World<TestDamageWorld>.Types()
            );

            World<TestDamageWorld>.Initialize();
        }

        [TearDown]
        public void TearDown()
        {
            if (World<TestDamageWorld>.Status != WorldStatus.NotCreated)
            {
                World<TestDamageWorld>.Destroy();
            }
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
