namespace UniGame.StaticEcs.Features.Tests
{
    using FFS.Libraries.StaticEcs;
    using NUnit.Framework;
    using UniGame.StaticEcs.Tests;

    [TestFixture]
    public sealed class ModifierClampAndRecomputeTests
    {
        private StaticEcsTestWorld<TestModifierWorld> _world;

        [SetUp]
        public void SetUp()
        {
            _world = new StaticEcsTestWorld<TestModifierWorld>();
            CharacteristicTypeRegistration.Register<TestModifierWorld, SpeedCharacteristic>(
                _world.Types);
            new ModifierBackRefFeature<TestModifierWorld>().InstallResourcesAndRegisterTypesForTest(
                _world
            );
            new CharacteristicFeature<TestModifierWorld, SpeedCharacteristic>().InstallResourcesAndRegisterTypesForTest(
                _world
            );
            _world.Initialize();
        }

        [TearDown]
        public void TearDown()
        {
            _world?.Dispose();
        }

        [Test]
        public void Modifier_PushingBeyondMax_IsClamped()
        {
            var target = World<TestModifierWorld>.NewEntity<Default>();
            target.Set(CharacteristicComponent<SpeedCharacteristic>.Create(10f, 0f, 12f));
            var source = World<TestModifierWorld>.NewEntity<Default>();

            CharacteristicModifierExtensions.ApplyModifier<TestModifierWorld, SpeedCharacteristic>(
                target.GID,
                source.GID,
                CharacteristicModifierOp.Add,
                100f
            );

            Assert.AreEqual(12f, target.Read<CharacteristicComponent<SpeedCharacteristic>>().Value);
        }

        [Test]
        public void RecomputeOnSetBaseValue_AppliesActiveModifiers()
        {
            var target = World<TestModifierWorld>.NewEntity<Default>();
            target.Set(CharacteristicComponent<SpeedCharacteristic>.Create(10f, 0f, 1000f));
            var source = World<TestModifierWorld>.NewEntity<Default>();

            CharacteristicModifierExtensions.ApplyModifier<TestModifierWorld, SpeedCharacteristic>(
                target.GID,
                source.GID,
                CharacteristicModifierOp.Mul,
                2f
            );

            Assert.AreEqual(20f, target.Read<CharacteristicComponent<SpeedCharacteristic>>().Value);

            ref var c = ref target.Ref<CharacteristicComponent<SpeedCharacteristic>>();
            CharacteristicOperations.SetBaseValue<TestModifierWorld, SpeedCharacteristic>(
                ref c,
                target.GID,
                25f,
                resetValue: true
            );

            CharacteristicModifierExtensions.RecomputeValue<TestModifierWorld, SpeedCharacteristic>(
                target.GID
            );

            Assert.AreEqual(
                25f,
                target.Read<CharacteristicComponent<SpeedCharacteristic>>().BaseValue
            );
            Assert.AreEqual(50f, target.Read<CharacteristicComponent<SpeedCharacteristic>>().Value);
        }
    }
}
