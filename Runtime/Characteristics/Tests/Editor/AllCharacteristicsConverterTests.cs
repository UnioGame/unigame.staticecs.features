namespace UniGame.StaticEcs.Features.Tests
{
    using FFS.Libraries.StaticEcs;
    using NUnit.Framework;
    using UniGame.StaticEcs.Tests;
    using UnityEngine;

    [TestFixture]
    public sealed class AllCharacteristicsConverterTests
    {
        private GameObject _host;
        private StaticEcsTestWorld<TestAllCharacteristicsWorld> _world;

        [SetUp]
        public void SetUp()
        {
            _world = new StaticEcsTestWorld<TestAllCharacteristicsWorld>();
            var types = _world.Types;
            CharacteristicTypeRegistration.Register<TestAllCharacteristicsWorld, HealthCharacteristic>(types);
            CharacteristicTypeRegistration.Register<TestAllCharacteristicsWorld, ManaCharacteristic>(types);
            CharacteristicTypeRegistration.Register<TestAllCharacteristicsWorld, SpeedCharacteristic>(types);
            CharacteristicTypeRegistration.Register<TestAllCharacteristicsWorld, ShieldCharacteristic>(types);
            CharacteristicTypeRegistration.Register<TestAllCharacteristicsWorld, BlockChanceCharacteristic>(types);
            CharacteristicTypeRegistration.Register<TestAllCharacteristicsWorld, DodgeChanceCharacteristic>(types);
            CharacteristicTypeRegistration.Register<TestAllCharacteristicsWorld, ArmorResistCharacteristic>(types);
            CharacteristicTypeRegistration.Register<TestAllCharacteristicsWorld, CriticalChanceCharacteristic>(types);
            CharacteristicTypeRegistration.Register<TestAllCharacteristicsWorld, CriticalMultiplierCharacteristic>(types);
            new ModifierBackRefFeature<TestAllCharacteristicsWorld>()
                .InstallResourcesAndRegisterTypesForTest(_world);
            new CharacteristicFeature<TestAllCharacteristicsWorld, HealthCharacteristic>().InstallResourcesAndRegisterTypesForTest(_world);
            new CharacteristicFeature<TestAllCharacteristicsWorld, ManaCharacteristic>().InstallResourcesAndRegisterTypesForTest(_world);
            new CharacteristicFeature<TestAllCharacteristicsWorld, SpeedCharacteristic>().InstallResourcesAndRegisterTypesForTest(_world);
            new CharacteristicFeature<TestAllCharacteristicsWorld, ShieldCharacteristic>().InstallResourcesAndRegisterTypesForTest(_world);
            new CharacteristicFeature<TestAllCharacteristicsWorld, ArmorResistCharacteristic>().InstallResourcesAndRegisterTypesForTest(_world);
            new CharacteristicFeature<TestAllCharacteristicsWorld, BlockChanceCharacteristic>().InstallResourcesAndRegisterTypesForTest(_world);
            new CharacteristicFeature<TestAllCharacteristicsWorld, DodgeChanceCharacteristic>().InstallResourcesAndRegisterTypesForTest(_world);
            new CharacteristicFeature<TestAllCharacteristicsWorld, CriticalChanceCharacteristic>().InstallResourcesAndRegisterTypesForTest(_world);
            new CharacteristicFeature<TestAllCharacteristicsWorld, CriticalMultiplierCharacteristic>().InstallResourcesAndRegisterTypesForTest(_world);
            _world.Initialize();
            _host = new GameObject(nameof(AllCharacteristicsConverterTests));
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_host);
            _world?.Dispose();
        }

        [Test]
        public void Apply_SetsAllNineCharacteristics()
        {
            var entity = World<TestAllCharacteristicsWorld>.NewEntity<Default>();
            var converter = _host.AddComponent<TestAllCharacteristicsConverter>();
            converter.Health = new CharacteristicSettings(80f, 10f, 120f);
            converter.Mana = new CharacteristicSettings(50f, 0f, 100f);
            converter.Speed = new CharacteristicSettings(5f, 0f, 20f);
            converter.Shield = new CharacteristicSettings(30f, 0f, 200f);
            converter.ArmorResist = new CharacteristicSettings(0.3f, 0f, 1f);
            converter.BlockChance = new CharacteristicSettings(0.1f, 0f, 1f);
            converter.DodgeChance = new CharacteristicSettings(0.2f, 0f, 1f);
            converter.CritChance = new CharacteristicSettings(0.15f, 0f, 1f);
            converter.CritMultiplier = new CharacteristicSettings(2.5f, 1f, 10f);

            converter.Apply(entity, null);

            Assert.AreEqual(
                80f,
                entity.Read<CharacteristicComponent<HealthCharacteristic>>().Value,
                1e-5f
            );
            Assert.AreEqual(
                10f,
                entity.Read<CharacteristicComponent<HealthCharacteristic>>().MinValue,
                1e-5f
            );
            Assert.AreEqual(
                120f,
                entity.Read<CharacteristicComponent<HealthCharacteristic>>().MaxValue,
                1e-5f
            );

            Assert.AreEqual(
                50f,
                entity.Read<CharacteristicComponent<ManaCharacteristic>>().Value,
                1e-5f
            );
            Assert.AreEqual(
                5f,
                entity.Read<CharacteristicComponent<SpeedCharacteristic>>().Value,
                1e-5f
            );
            Assert.AreEqual(
                30f,
                entity.Read<CharacteristicComponent<ShieldCharacteristic>>().Value,
                1e-5f
            );
            Assert.AreEqual(
                0.3f,
                entity.Read<CharacteristicComponent<ArmorResistCharacteristic>>().Value,
                1e-5f
            );
            Assert.AreEqual(
                0.1f,
                entity.Read<CharacteristicComponent<BlockChanceCharacteristic>>().Value,
                1e-5f
            );
            Assert.AreEqual(
                0.2f,
                entity.Read<CharacteristicComponent<DodgeChanceCharacteristic>>().Value,
                1e-5f
            );
            Assert.AreEqual(
                0.15f,
                entity.Read<CharacteristicComponent<CriticalChanceCharacteristic>>().Value,
                1e-5f
            );
            Assert.AreEqual(
                2.5f,
                entity.Read<CharacteristicComponent<CriticalMultiplierCharacteristic>>().Value,
                1e-5f
            );
        }

        [Test]
        public void Apply_DefaultSettings_ValuesMatchDefaults()
        {
            var entity = World<TestAllCharacteristicsWorld>.NewEntity<Default>();
            var converter = _host.AddComponent<TestAllCharacteristicsConverter>();

            converter.Apply(entity, null);

            Assert.AreEqual(
                100f,
                entity.Read<CharacteristicComponent<HealthCharacteristic>>().Value,
                1e-5f
            );
            Assert.AreEqual(
                0f,
                entity.Read<CharacteristicComponent<HealthCharacteristic>>().MinValue,
                1e-5f
            );
            Assert.AreEqual(
                100f,
                entity.Read<CharacteristicComponent<HealthCharacteristic>>().MaxValue,
                1e-5f
            );
            Assert.AreEqual(
                2f,
                entity.Read<CharacteristicComponent<CriticalMultiplierCharacteristic>>().Value,
                1e-5f
            );
            Assert.AreEqual(
                1f,
                entity.Read<CharacteristicComponent<CriticalMultiplierCharacteristic>>().MinValue,
                1e-5f
            );
        }

        [Test]
        public void Apply_AssetVariant_SetsAllNineCharacteristics()
        {
            var entity = World<TestAllCharacteristicsWorld>.NewEntity<Default>();
            var asset = ScriptableObject.CreateInstance<TestAllCharacteristicsConverterAsset>();
            asset.health = new CharacteristicSettings(60f, 0f, 100f);
            asset.mana = new CharacteristicSettings(40f, 0f, 80f);
            asset.speed = new CharacteristicSettings(3f, 0f, 15f);
            asset.shield = new CharacteristicSettings(10f, 0f, 150f);
            asset.armorResist = new CharacteristicSettings(0.1f, 0f, 1f);
            asset.blockChance = new CharacteristicSettings(0.05f, 0f, 1f);
            asset.dodgeChance = new CharacteristicSettings(0f, 0f, 1f);
            asset.critChance = new CharacteristicSettings(0.1f, 0f, 1f);
            asset.critMultiplier = new CharacteristicSettings(3f, 1f, 5f);

            asset.Apply(entity, null);

            Assert.AreEqual(
                60f,
                entity.Read<CharacteristicComponent<HealthCharacteristic>>().Value,
                1e-5f
            );
            Assert.AreEqual(
                40f,
                entity.Read<CharacteristicComponent<ManaCharacteristic>>().Value,
                1e-5f
            );
            Assert.AreEqual(
                3f,
                entity.Read<CharacteristicComponent<SpeedCharacteristic>>().Value,
                1e-5f
            );
            Assert.AreEqual(
                10f,
                entity.Read<CharacteristicComponent<ShieldCharacteristic>>().Value,
                1e-5f
            );
            Assert.AreEqual(
                0.1f,
                entity.Read<CharacteristicComponent<ArmorResistCharacteristic>>().Value,
                1e-5f
            );
            Assert.AreEqual(
                0.05f,
                entity.Read<CharacteristicComponent<BlockChanceCharacteristic>>().Value,
                1e-5f
            );
            Assert.AreEqual(
                0f,
                entity.Read<CharacteristicComponent<DodgeChanceCharacteristic>>().Value,
                1e-5f
            );
            Assert.AreEqual(
                0.1f,
                entity.Read<CharacteristicComponent<CriticalChanceCharacteristic>>().Value,
                1e-5f
            );
            Assert.AreEqual(
                3f,
                entity.Read<CharacteristicComponent<CriticalMultiplierCharacteristic>>().Value,
                1e-5f
            );

            Object.DestroyImmediate(asset);
        }

        [Test]
        public void Apply_SerializableVariant_SetsAllNineCharacteristics()
        {
            var entity = World<TestAllCharacteristicsWorld>.NewEntity<Default>();
            var converter = new TestAllCharacteristicsSerializableConverter
            {
                health = new CharacteristicSettings(75f, 0f, 100f),
                mana = new CharacteristicSettings(35f, 0f, 80f),
                speed = new CharacteristicSettings(9f, 0f, 50f),
                shield = new CharacteristicSettings(20f, 0f, 150f),
                armorResist = new CharacteristicSettings(0.2f, 0f, 1f),
                blockChance = new CharacteristicSettings(0.1f, 0f, 1f),
                dodgeChance = new CharacteristicSettings(0.15f, 0f, 1f),
                critChance = new CharacteristicSettings(0.25f, 0f, 1f),
                critMultiplier = new CharacteristicSettings(4f, 1f, 6f),
            };

            converter.Apply(entity, _host);

            Assert.AreEqual(
                75f,
                entity.Read<CharacteristicComponent<HealthCharacteristic>>().Value,
                1e-5f
            );
            Assert.AreEqual(
                35f,
                entity.Read<CharacteristicComponent<ManaCharacteristic>>().Value,
                1e-5f
            );
            Assert.AreEqual(
                9f,
                entity.Read<CharacteristicComponent<SpeedCharacteristic>>().Value,
                1e-5f
            );
            Assert.AreEqual(
                50f,
                entity.Read<CharacteristicComponent<SpeedCharacteristic>>().MaxValue,
                1e-5f
            );
            Assert.AreEqual(
                4f,
                entity.Read<CharacteristicComponent<CriticalMultiplierCharacteristic>>().Value,
                1e-5f
            );
        }
    }

    internal sealed class TestAllCharacteristicsConverter
        : AllCharacteristicsConverter<TestAllCharacteristicsWorld> { }

    internal sealed class TestAllCharacteristicsConverterAsset
        : AllCharacteristicsConverterAsset<TestAllCharacteristicsWorld> { }

    [System.Serializable]
    internal sealed class TestAllCharacteristicsSerializableConverter
        : AllCharacteristicsSerializableConverter<TestAllCharacteristicsWorld> { }
}
