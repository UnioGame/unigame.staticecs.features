using FFS.Libraries.StaticEcs;
using NUnit.Framework;
using UnityEngine;

namespace UniGame.StaticEcs.Features.Tests {
    [TestFixture]
    public sealed class AllCharacteristicsConverterTests {
        private GameObject _host;

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
            _host = new GameObject(nameof(AllCharacteristicsConverterTests));
        }

        [TearDown]
        public void TearDown() {
            Object.DestroyImmediate(_host);
            if (World<TestAllCharacteristicsWorld>.Status != WorldStatus.NotCreated) {
                World<TestAllCharacteristicsWorld>.Destroy();
            }
        }

        [Test]
        public void Apply_SetsAllNineCharacteristics() {
            var entity = World<TestAllCharacteristicsWorld>.NewEntity<Default>();
            var converter = _host.AddComponent<TestAllCharacteristicsConverter>();
            converter.Health         = new CharacteristicSettings(80f,  10f, 120f);
            converter.Mana           = new CharacteristicSettings(50f,  0f,  100f);
            converter.Speed          = new CharacteristicSettings(5f,   0f,  20f);
            converter.Shield         = new CharacteristicSettings(30f,  0f,  200f);
            converter.ArmorResist    = new CharacteristicSettings(0.3f, 0f,  1f);
            converter.BlockChance    = new CharacteristicSettings(0.1f, 0f,  1f);
            converter.DodgeChance    = new CharacteristicSettings(0.2f, 0f,  1f);
            converter.CritChance     = new CharacteristicSettings(0.15f,0f,  1f);
            converter.CritMultiplier = new CharacteristicSettings(2.5f, 1f,  10f);

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
            var converter = _host.AddComponent<TestAllCharacteristicsConverter>();

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
            var asset  = ScriptableObject.CreateInstance<TestAllCharacteristicsConverterAsset>();
            asset.health         = new CharacteristicSettings(60f, 0f, 100f);
            asset.mana           = new CharacteristicSettings(40f, 0f, 80f);
            asset.speed          = new CharacteristicSettings(3f,  0f, 15f);
            asset.shield         = new CharacteristicSettings(10f, 0f, 150f);
            asset.armorResist    = new CharacteristicSettings(0.1f,0f, 1f);
            asset.blockChance    = new CharacteristicSettings(0.05f,0f,1f);
            asset.dodgeChance    = new CharacteristicSettings(0f,  0f, 1f);
            asset.critChance     = new CharacteristicSettings(0.1f,0f, 1f);
            asset.critMultiplier = new CharacteristicSettings(3f,  1f, 5f);

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

            Object.DestroyImmediate(asset);
        }
    }

    internal sealed class TestAllCharacteristicsConverter : AllCharacteristicsConverter<TestAllCharacteristicsWorld> { }

    internal sealed class TestAllCharacteristicsConverterAsset : AllCharacteristicsConverterAsset<TestAllCharacteristicsWorld> { }
}
