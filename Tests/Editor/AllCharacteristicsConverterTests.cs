using FFS.Libraries.StaticEcs;
using NUnit.Framework;

namespace UniGame.StaticEcs.Features.Tests {
    [TestFixture]
    public sealed class AllCharacteristicsConverterTests {
        [SetUp]
        public void SetUp() {
            World<TestAllCharacteristicsWorld>.Create(WorldConfig.Default());
            var types = World<TestAllCharacteristicsWorld>.Types();
            new ModifierBackRefFeature<TestAllCharacteristicsWorld>().RegisterTypes(types);
            new CharacteristicFeature<TestAllCharacteristicsWorld, HealthCharacteristic>().RegisterTypes(types);
            new CharacteristicFeature<TestAllCharacteristicsWorld, ManaCharacteristic>().RegisterTypes(types);
            new CharacteristicFeature<TestAllCharacteristicsWorld, SpeedCharacteristic>().RegisterTypes(types);
            new CharacteristicFeature<TestAllCharacteristicsWorld, ShieldCharacteristic>().RegisterTypes(types);
            new CharacteristicFeature<TestAllCharacteristicsWorld, ArmorResistCharacteristic>().RegisterTypes(types);
            new CharacteristicFeature<TestAllCharacteristicsWorld, BlockChanceCharacteristic>().RegisterTypes(types);
            new CharacteristicFeature<TestAllCharacteristicsWorld, DodgeChanceCharacteristic>().RegisterTypes(types);
            new CharacteristicFeature<TestAllCharacteristicsWorld, CriticalChanceCharacteristic>().RegisterTypes(types);
            new CharacteristicFeature<TestAllCharacteristicsWorld, CriticalMultiplierCharacteristic>().RegisterTypes(types);
            World<TestAllCharacteristicsWorld>.Initialize();
        }

        [TearDown]
        public void TearDown() {
            if (World<TestAllCharacteristicsWorld>.Status != WorldStatus.NotCreated) {
                World<TestAllCharacteristicsWorld>.Destroy();
            }
        }

        [Test]
        public void Apply_SetsAllNineCharacteristics() {
            var entity = World<TestAllCharacteristicsWorld>.NewEntity<Default>();
            var converter = new AllCharacteristicsConverter<TestAllCharacteristicsWorld> {
                Health         = new CharacteristicSettings(80f,  10f, 120f),
                Mana           = new CharacteristicSettings(50f,  0f,  100f),
                Speed          = new CharacteristicSettings(5f,   0f,  20f),
                Shield         = new CharacteristicSettings(30f,  0f,  200f),
                ArmorResist    = new CharacteristicSettings(0.3f, 0f,  1f),
                BlockChance    = new CharacteristicSettings(0.1f, 0f,  1f),
                DodgeChance    = new CharacteristicSettings(0.2f, 0f,  1f),
                CritChance     = new CharacteristicSettings(0.15f,0f,  1f),
                CritMultiplier = new CharacteristicSettings(2.5f, 1f,  10f),
            };

            converter.Apply(entity, null);

            Assert.AreEqual(80f,   entity.Read<CharacteristicComponent<HealthCharacteristic>>().Value,         1e-5f);
            Assert.AreEqual(10f,   entity.Read<CharacteristicComponent<HealthCharacteristic>>().MinValue,      1e-5f);
            Assert.AreEqual(120f,  entity.Read<CharacteristicComponent<HealthCharacteristic>>().MaxValue,      1e-5f);

            Assert.AreEqual(50f,   entity.Read<CharacteristicComponent<ManaCharacteristic>>().Value,           1e-5f);
            Assert.AreEqual(5f,    entity.Read<CharacteristicComponent<SpeedCharacteristic>>().Value,          1e-5f);
            Assert.AreEqual(30f,   entity.Read<CharacteristicComponent<ShieldCharacteristic>>().Value,         1e-5f);
            Assert.AreEqual(0.3f,  entity.Read<CharacteristicComponent<ArmorResistCharacteristic>>().Value,    1e-5f);
            Assert.AreEqual(0.1f,  entity.Read<CharacteristicComponent<BlockChanceCharacteristic>>().Value,    1e-5f);
            Assert.AreEqual(0.2f,  entity.Read<CharacteristicComponent<DodgeChanceCharacteristic>>().Value,    1e-5f);
            Assert.AreEqual(0.15f, entity.Read<CharacteristicComponent<CriticalChanceCharacteristic>>().Value, 1e-5f);
            Assert.AreEqual(2.5f,  entity.Read<CharacteristicComponent<CriticalMultiplierCharacteristic>>().Value, 1e-5f);
        }

        [Test]
        public void Apply_DefaultSettings_ValuesMatchDefaults() {
            var entity    = World<TestAllCharacteristicsWorld>.NewEntity<Default>();
            var converter = new AllCharacteristicsConverter<TestAllCharacteristicsWorld>();

            converter.Apply(entity, null);

            Assert.AreEqual(100f, entity.Read<CharacteristicComponent<HealthCharacteristic>>().Value,          1e-5f);
            Assert.AreEqual(0f,   entity.Read<CharacteristicComponent<HealthCharacteristic>>().MinValue,       1e-5f);
            Assert.AreEqual(100f, entity.Read<CharacteristicComponent<HealthCharacteristic>>().MaxValue,       1e-5f);
            Assert.AreEqual(2f,   entity.Read<CharacteristicComponent<CriticalMultiplierCharacteristic>>().Value, 1e-5f);
            Assert.AreEqual(1f,   entity.Read<CharacteristicComponent<CriticalMultiplierCharacteristic>>().MinValue, 1e-5f);
        }

        [Test]
        public void Apply_AssetVariant_SetsAllNineCharacteristics() {
            var entity = World<TestAllCharacteristicsWorld>.NewEntity<Default>();
            var asset  = new AllCharacteristicsConverterAsset<TestAllCharacteristicsWorld> {
                health         = new CharacteristicSettings(60f, 0f, 100f),
                mana           = new CharacteristicSettings(40f, 0f, 80f),
                speed          = new CharacteristicSettings(3f,  0f, 15f),
                shield         = new CharacteristicSettings(10f, 0f, 150f),
                armorResist    = new CharacteristicSettings(0.1f,0f, 1f),
                blockChance    = new CharacteristicSettings(0.05f,0f,1f),
                dodgeChance    = new CharacteristicSettings(0f,  0f, 1f),
                critChance     = new CharacteristicSettings(0.1f,0f, 1f),
                critMultiplier = new CharacteristicSettings(3f,  1f, 5f),
            };

            asset.Apply(entity, null);

            Assert.AreEqual(60f,   entity.Read<CharacteristicComponent<HealthCharacteristic>>().Value,         1e-5f);
            Assert.AreEqual(40f,   entity.Read<CharacteristicComponent<ManaCharacteristic>>().Value,           1e-5f);
            Assert.AreEqual(3f,    entity.Read<CharacteristicComponent<SpeedCharacteristic>>().Value,          1e-5f);
            Assert.AreEqual(10f,   entity.Read<CharacteristicComponent<ShieldCharacteristic>>().Value,         1e-5f);
            Assert.AreEqual(0.1f,  entity.Read<CharacteristicComponent<ArmorResistCharacteristic>>().Value,    1e-5f);
            Assert.AreEqual(0.05f, entity.Read<CharacteristicComponent<BlockChanceCharacteristic>>().Value,    1e-5f);
            Assert.AreEqual(0f,    entity.Read<CharacteristicComponent<DodgeChanceCharacteristic>>().Value,    1e-5f);
            Assert.AreEqual(0.1f,  entity.Read<CharacteristicComponent<CriticalChanceCharacteristic>>().Value, 1e-5f);
            Assert.AreEqual(3f,    entity.Read<CharacteristicComponent<CriticalMultiplierCharacteristic>>().Value, 1e-5f);
        }
    }
}
