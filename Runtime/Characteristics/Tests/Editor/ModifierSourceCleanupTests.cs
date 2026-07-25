namespace UniGame.StaticEcs.Features.Tests
{
    using FFS.Libraries.StaticEcs;
    using NUnit.Framework;
    using UniGame.StaticEcs.Tests;

    [TestFixture]
    public sealed class ModifierSourceCleanupTests
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
        public void DestroySource_RemovesItsModifiersOnAllTargets_KeepsOtherSources()
        {
            var targetA = NewTarget(10f);
            var targetB = NewTarget(20f);
            var sourceA = World<TestModifierWorld>.NewEntity<Default>();
            var sourceB = World<TestModifierWorld>.NewEntity<Default>();

            CharacteristicModifierExtensions.ApplyModifier<TestModifierWorld, SpeedCharacteristic>(
                targetA.GID,
                sourceA.GID,
                CharacteristicModifierOp.Add,
                5f
            );
            CharacteristicModifierExtensions.ApplyModifier<TestModifierWorld, SpeedCharacteristic>(
                targetB.GID,
                sourceA.GID,
                CharacteristicModifierOp.Add,
                7f
            );
            CharacteristicModifierExtensions.ApplyModifier<TestModifierWorld, SpeedCharacteristic>(
                targetA.GID,
                sourceB.GID,
                CharacteristicModifierOp.Add,
                1f
            );

            Assert.AreEqual(
                16f,
                targetA.Read<CharacteristicComponent<SpeedCharacteristic>>().Value
            );
            Assert.AreEqual(
                27f,
                targetB.Read<CharacteristicComponent<SpeedCharacteristic>>().Value
            );

            sourceA.Destroy();

            Assert.AreEqual(
                11f,
                targetA.Read<CharacteristicComponent<SpeedCharacteristic>>().Value
            );
            Assert.AreEqual(
                20f,
                targetB.Read<CharacteristicComponent<SpeedCharacteristic>>().Value
            );
        }

        private static World<TestModifierWorld>.Entity NewTarget(float baseValue)
        {
            var entity = World<TestModifierWorld>.NewEntity<Default>();
            entity.Set(CharacteristicComponent<SpeedCharacteristic>.Create(baseValue, 0f, 1000f));
            return entity;
        }
    }
}
