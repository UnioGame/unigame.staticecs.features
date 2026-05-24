using FFS.Libraries.StaticEcs;
using NUnit.Framework;

namespace unigame.staticecs.features.Tests {
    public sealed class DamageFilterTests {
        private FakeDamageRng _rng;

        [SetUp]
        public void SetUp() {
            World<TestDamageWorld>.Create(WorldConfig.Default());
            _rng = new FakeDamageRng();
            World<TestDamageWorld>.SetResource<IDamageRng>(_rng);

            new ModifierBackRefFeature<TestDamageWorld>().RegisterTypes(World<TestDamageWorld>.Types());
            new CharacteristicFeature<TestDamageWorld, HealthCharacteristic>().RegisterTypes(World<TestDamageWorld>.Types());
            new CharacteristicFeature<TestDamageWorld, ShieldCharacteristic>().RegisterTypes(World<TestDamageWorld>.Types());
            new CharacteristicFeature<TestDamageWorld, BlockChanceCharacteristic>().RegisterTypes(World<TestDamageWorld>.Types());
            new CharacteristicFeature<TestDamageWorld, DodgeChanceCharacteristic>().RegisterTypes(World<TestDamageWorld>.Types());
            new CharacteristicFeature<TestDamageWorld, ArmorResistCharacteristic>().RegisterTypes(World<TestDamageWorld>.Types());
            new CharacteristicFeature<TestDamageWorld, CriticalChanceCharacteristic>().RegisterTypes(World<TestDamageWorld>.Types());
            new CharacteristicFeature<TestDamageWorld, CriticalMultiplierCharacteristic>().RegisterTypes(World<TestDamageWorld>.Types());
            new DamageFeature<TestDamageWorld>(registerApplySystem: false).RegisterTypes(World<TestDamageWorld>.Types());

            World<TestDamageWorld>.Initialize();
        }

        [TearDown]
        public void TearDown() {
            if (World<TestDamageWorld>.Status != WorldStatus.NotCreated) {
                World<TestDamageWorld>.Destroy();
            }
        }

        private static DamageContext MakeCtx(EntityGID source, EntityGID target, float amount, DamageType type = DamageType.Physical, bool forceCrit = false) {
            return DamageContext.FromEvent(new IncomingDamageEvent {
                Source = source,
                Target = target,
                Amount = amount,
                Type = type,
                ForceCritical = forceCrit
            });
        }

        [Test]
        public void Dodge_RollSucceeds_CancelsAndMarksDodged() {
            var source = World<TestDamageWorld>.NewEntity<Default>();
            var target = World<TestDamageWorld>.NewEntity<Default>();
            target.Set(CharacteristicComponent<DodgeChanceCharacteristic>.Create(0.5f, 0f, 1f));
            _rng.NextRoll = true;

            var ctx = MakeCtx(source.GID, target.GID, 10f);
            new DodgeFilter<TestDamageWorld>().Apply(ref ctx);

            Assert.IsTrue(ctx.Cancelled);
            Assert.AreEqual(DamageCancelReason.Dodged, ctx.CancelReason);
        }

        [Test]
        public void Dodge_RollFails_DoesNothing() {
            var source = World<TestDamageWorld>.NewEntity<Default>();
            var target = World<TestDamageWorld>.NewEntity<Default>();
            target.Set(CharacteristicComponent<DodgeChanceCharacteristic>.Create(0.5f, 0f, 1f));
            _rng.NextRoll = false;

            var ctx = MakeCtx(source.GID, target.GID, 10f);
            new DodgeFilter<TestDamageWorld>().Apply(ref ctx);

            Assert.IsFalse(ctx.Cancelled);
        }

        [Test]
        public void Dodge_HealingType_Skipped() {
            var source = World<TestDamageWorld>.NewEntity<Default>();
            var target = World<TestDamageWorld>.NewEntity<Default>();
            target.Set(CharacteristicComponent<DodgeChanceCharacteristic>.Create(1f, 0f, 1f));
            _rng.NextRoll = true;

            var ctx = MakeCtx(source.GID, target.GID, 10f, DamageType.Healing);
            new DodgeFilter<TestDamageWorld>().Apply(ref ctx);

            Assert.IsFalse(ctx.Cancelled);
        }

        [Test]
        public void Block_RequiresBlockableTag() {
            var source = World<TestDamageWorld>.NewEntity<Default>();
            var target = World<TestDamageWorld>.NewEntity<Default>();
            target.Set(CharacteristicComponent<BlockChanceCharacteristic>.Create(1f, 0f, 1f));
            _rng.NextRoll = true;

            var ctx = MakeCtx(source.GID, target.GID, 10f);
            new BlockFilter<TestDamageWorld>().Apply(ref ctx);

            Assert.IsFalse(ctx.Cancelled);
        }

        [Test]
        public void Block_RollSucceeds_CancelsAndMarksBlocked() {
            var source = World<TestDamageWorld>.NewEntity<Default>();
            var target = World<TestDamageWorld>.NewEntity<Default>();
            target.Set<BlockableTag>();
            target.Set(CharacteristicComponent<BlockChanceCharacteristic>.Create(0.5f, 0f, 1f));
            _rng.NextRoll = true;

            var ctx = MakeCtx(source.GID, target.GID, 10f);
            new BlockFilter<TestDamageWorld>().Apply(ref ctx);

            Assert.IsTrue(ctx.Cancelled);
            Assert.AreEqual(DamageCancelReason.Blocked, ctx.CancelReason);
        }

        [Test]
        public void ArmorResist_PhysicalOnly_ReducesAmount() {
            var source = World<TestDamageWorld>.NewEntity<Default>();
            var target = World<TestDamageWorld>.NewEntity<Default>();
            target.Set(CharacteristicComponent<ArmorResistCharacteristic>.Create(0.25f, 0f, 1f));

            var ctx = MakeCtx(source.GID, target.GID, 100f, DamageType.Physical);
            new ArmorResistFilter<TestDamageWorld>().Apply(ref ctx);

            Assert.AreEqual(75f, ctx.Amount, 0.0001f);
        }

        [Test]
        public void ArmorResist_NonPhysical_Untouched() {
            var source = World<TestDamageWorld>.NewEntity<Default>();
            var target = World<TestDamageWorld>.NewEntity<Default>();
            target.Set(CharacteristicComponent<ArmorResistCharacteristic>.Create(0.5f, 0f, 1f));

            var ctx = MakeCtx(source.GID, target.GID, 100f, DamageType.Magical);
            new ArmorResistFilter<TestDamageWorld>().Apply(ref ctx);

            Assert.AreEqual(100f, ctx.Amount, 0.0001f);
        }

        [Test]
        public void Critical_ForceFlag_AppliesDefaultMultiplier() {
            var source = World<TestDamageWorld>.NewEntity<Default>();
            var target = World<TestDamageWorld>.NewEntity<Default>();

            var ctx = MakeCtx(source.GID, target.GID, 10f, forceCrit: true);
            new CriticalFilter<TestDamageWorld>().Apply(ref ctx);

            Assert.IsTrue(ctx.IsCritical);
            Assert.AreEqual(20f, ctx.Amount, 0.0001f);
        }

        [Test]
        public void Critical_RollSucceeds_UsesSourceMultiplier() {
            var source = World<TestDamageWorld>.NewEntity<Default>();
            var target = World<TestDamageWorld>.NewEntity<Default>();
            source.Set(CharacteristicComponent<CriticalChanceCharacteristic>.Create(0.5f, 0f, 1f));
            source.Set(CharacteristicComponent<CriticalMultiplierCharacteristic>.Create(3f, 0f, 10f));
            _rng.NextRoll = true;

            var ctx = MakeCtx(source.GID, target.GID, 10f);
            new CriticalFilter<TestDamageWorld>().Apply(ref ctx);

            Assert.IsTrue(ctx.IsCritical);
            Assert.AreEqual(30f, ctx.Amount, 0.0001f);
        }

        [Test]
        public void Critical_RollFails_NoEffect() {
            var source = World<TestDamageWorld>.NewEntity<Default>();
            var target = World<TestDamageWorld>.NewEntity<Default>();
            source.Set(CharacteristicComponent<CriticalChanceCharacteristic>.Create(0.5f, 0f, 1f));
            _rng.NextRoll = false;

            var ctx = MakeCtx(source.GID, target.GID, 10f);
            new CriticalFilter<TestDamageWorld>().Apply(ref ctx);

            Assert.IsFalse(ctx.IsCritical);
            Assert.AreEqual(10f, ctx.Amount, 0.0001f);
        }

        [Test]
        public void Shield_AbsorbsPartial_ReducesAmount() {
            var source = World<TestDamageWorld>.NewEntity<Default>();
            var target = World<TestDamageWorld>.NewEntity<Default>();
            target.Set(CharacteristicComponent<ShieldCharacteristic>.Create(30f, 0f, 100f));

            var ctx = MakeCtx(source.GID, target.GID, 50f);
            new ShieldFilter<TestDamageWorld>().Apply(ref ctx);

            Assert.AreEqual(20f, ctx.Amount, 0.0001f);
            Assert.AreEqual(30f, ctx.ShieldAbsorbed, 0.0001f);
            Assert.AreEqual(0f, target.Read<CharacteristicComponent<ShieldCharacteristic>>().Value);
        }

        [Test]
        public void Shield_AbsorbsFull_ZerosOutAmount() {
            var source = World<TestDamageWorld>.NewEntity<Default>();
            var target = World<TestDamageWorld>.NewEntity<Default>();
            target.Set(CharacteristicComponent<ShieldCharacteristic>.Create(100f, 0f, 100f));

            var ctx = MakeCtx(source.GID, target.GID, 50f);
            new ShieldFilter<TestDamageWorld>().Apply(ref ctx);

            Assert.AreEqual(0f, ctx.Amount, 0.0001f);
            Assert.AreEqual(50f, ctx.ShieldAbsorbed, 0.0001f);
            Assert.AreEqual(50f, target.Read<CharacteristicComponent<ShieldCharacteristic>>().Value);
        }
    }
}
