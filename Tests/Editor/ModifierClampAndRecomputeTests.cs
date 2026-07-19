using FFS.Libraries.StaticEcs;
using NUnit.Framework;

namespace UniGame.StaticEcs.Features.Tests {
    [TestFixture]
    public sealed class ModifierClampAndRecomputeTests {
        [SetUp]
        public void SetUp() {
            World<TestModifierWorld>.Create(WorldConfig.Default());
            new ModifierBackRefFeature<TestModifierWorld>().RegisterTypes(World<TestModifierWorld>.Types());
            new CharacteristicFeature<TestModifierWorld, SpeedCharacteristic>().RegisterTypes(World<TestModifierWorld>.Types());
            World<TestModifierWorld>.Initialize();
        }

        [TearDown]
        public void TearDown() {
            if (World<TestModifierWorld>.Status != WorldStatus.NotCreated) {
                World<TestModifierWorld>.Destroy();
            }
        }

        [Test]
        public void Modifier_PushingBeyondMax_IsClamped() {
            var target = World<TestModifierWorld>.NewEntity<Default>();
            target.Set(CharacteristicComponent<SpeedCharacteristic>.Create(10f, 0f, 12f));
            var source = World<TestModifierWorld>.NewEntity<Default>();

            CharacteristicModifierExtensions.ApplyModifier<TestModifierWorld, SpeedCharacteristic>(
                target.GID, source.GID, CharacteristicModifierOp.Add, 100f);

            Assert.AreEqual(12f, target.Read<CharacteristicComponent<SpeedCharacteristic>>().Value);
        }

        [Test]
        public void RecomputeOnSetBaseValue_AppliesActiveModifiers() {
            var target = World<TestModifierWorld>.NewEntity<Default>();
            target.Set(CharacteristicComponent<SpeedCharacteristic>.Create(10f, 0f, 1000f));
            var source = World<TestModifierWorld>.NewEntity<Default>();

            CharacteristicModifierExtensions.ApplyModifier<TestModifierWorld, SpeedCharacteristic>(
                target.GID, source.GID, CharacteristicModifierOp.Mul, 2f);

            Assert.AreEqual(20f, target.Read<CharacteristicComponent<SpeedCharacteristic>>().Value);

            ref var c = ref target.Ref<CharacteristicComponent<SpeedCharacteristic>>();
            CharacteristicOperations.SetBaseValue<TestModifierWorld, SpeedCharacteristic>(ref c, target.GID, 25f, resetValue: true);

            CharacteristicModifierExtensions.RecomputeValue<TestModifierWorld, SpeedCharacteristic>(target.GID);

            Assert.AreEqual(25f, target.Read<CharacteristicComponent<SpeedCharacteristic>>().BaseValue);
            Assert.AreEqual(50f, target.Read<CharacteristicComponent<SpeedCharacteristic>>().Value);
        }
    }
}
