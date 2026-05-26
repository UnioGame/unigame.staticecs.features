using FFS.Libraries.StaticEcs;
using NUnit.Framework;

namespace unigame.staticecs.features.Tests {
    [TestFixture]
    public sealed class ApplyDamageSystemTests {
        private FakeDamageRng _rng;
        private ApplyDamageSystem<TestDamageWorld> _system;

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

            _system = new ApplyDamageSystem<TestDamageWorld>();
            _system.Init();
        }

        [TearDown]
        public void TearDown() {
            if (World<TestDamageWorld>.Status != WorldStatus.NotCreated) {
                _system.Destroy();
                World<TestDamageWorld>.Destroy();
            }
        }

        [Test]
        public void RaiseDamage_AppliesToHealth_NoFilters() {
            var source = World<TestDamageWorld>.NewEntity<Default>();
            var target = World<TestDamageWorld>.NewEntity<Default>();
            target.Set(CharacteristicComponent<HealthCharacteristic>.Create(100f, 0f, 100f));

            DamageOperations.RaiseDamage<TestDamageWorld>(source.GID, target.GID, 30f);
            _system.Update();

            Assert.AreEqual(70f, target.Read<CharacteristicComponent<HealthCharacteristic>>().Value);
            Assert.IsFalse(target.Has<DeathPendingTag>());
        }

        [Test]
        public void LethalHit_SetsDeathPendingTag() {
            var source = World<TestDamageWorld>.NewEntity<Default>();
            var target = World<TestDamageWorld>.NewEntity<Default>();
            target.Set(CharacteristicComponent<HealthCharacteristic>.Create(20f, 0f, 100f));

            DamageOperations.RaiseDamage<TestDamageWorld>(source.GID, target.GID, 50f);
            _system.Update();

            Assert.AreEqual(0f, target.Read<CharacteristicComponent<HealthCharacteristic>>().Value);
            Assert.IsTrue(target.Has<DeathPendingTag>());
        }

        [Test]
        public void RaiseHealing_IncreasesHealth() {
            var source = World<TestDamageWorld>.NewEntity<Default>();
            var target = World<TestDamageWorld>.NewEntity<Default>();
            target.Set(CharacteristicComponent<HealthCharacteristic>.Create(40f, 0f, 100f));

            DamageOperations.RaiseHealing<TestDamageWorld>(source.GID, target.GID, 25f);
            _system.Update();

            Assert.AreEqual(65f, target.Read<CharacteristicComponent<HealthCharacteristic>>().Value);
        }

        [Test]
        public void DodgedDamage_DoesNotChangeHealth() {
            var source = World<TestDamageWorld>.NewEntity<Default>();
            var target = World<TestDamageWorld>.NewEntity<Default>();
            target.Set(CharacteristicComponent<HealthCharacteristic>.Create(100f, 0f, 100f));
            target.Set(CharacteristicComponent<DodgeChanceCharacteristic>.Create(1f, 0f, 1f));
            _rng.NextRoll = true;

            DamageOperations.RaiseDamage<TestDamageWorld>(source.GID, target.GID, 30f);
            _system.Update();

            Assert.AreEqual(100f, target.Read<CharacteristicComponent<HealthCharacteristic>>().Value);
        }
    }
}
