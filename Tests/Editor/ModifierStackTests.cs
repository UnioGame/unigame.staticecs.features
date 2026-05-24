using FFS.Libraries.StaticEcs;
using NUnit.Framework;

namespace unigame.staticecs.features.Tests {
    public sealed class ModifierStackTests {
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
        public void EmptyStack_ValueEqualsBase() {
            var target = NewTargetEntity(10f);
            var source = World<TestModifierWorld>.NewEntity<Default>();

            CharacteristicModifierExtensions.ApplyModifier<TestModifierWorld, SpeedCharacteristic>(
                target.GID, source.GID, CharacteristicModifierOp.Add, 5f);
            CharacteristicModifierExtensions.RemoveModifiersFromSource<TestModifierWorld, SpeedCharacteristic>(
                target.GID, source.GID);

            Assert.AreEqual(10f, target.Read<CharacteristicComponent<SpeedCharacteristic>>().Value);
        }

        [Test]
        public void Add_Modifier_ShiftsValue() {
            var target = NewTargetEntity(10f);
            var source = World<TestModifierWorld>.NewEntity<Default>();

            CharacteristicModifierExtensions.ApplyModifier<TestModifierWorld, SpeedCharacteristic>(
                target.GID, source.GID, CharacteristicModifierOp.Add, 5f);

            Assert.AreEqual(15f, target.Read<CharacteristicComponent<SpeedCharacteristic>>().Value);
        }

        [Test]
        public void Mul_Modifier_AppliesAfterAdd() {
            var target = NewTargetEntity(10f);
            var sourceA = World<TestModifierWorld>.NewEntity<Default>();
            var sourceB = World<TestModifierWorld>.NewEntity<Default>();

            CharacteristicModifierExtensions.ApplyModifier<TestModifierWorld, SpeedCharacteristic>(
                target.GID, sourceA.GID, CharacteristicModifierOp.Add, 10f);
            CharacteristicModifierExtensions.ApplyModifier<TestModifierWorld, SpeedCharacteristic>(
                target.GID, sourceB.GID, CharacteristicModifierOp.Mul, 2f);

            Assert.AreEqual(40f, target.Read<CharacteristicComponent<SpeedCharacteristic>>().Value);
        }

        [Test]
        public void Override_TakesPrecedence() {
            var target = NewTargetEntity(10f);
            var sourceA = World<TestModifierWorld>.NewEntity<Default>();
            var sourceB = World<TestModifierWorld>.NewEntity<Default>();

            CharacteristicModifierExtensions.ApplyModifier<TestModifierWorld, SpeedCharacteristic>(
                target.GID, sourceA.GID, CharacteristicModifierOp.Add, 999f);
            CharacteristicModifierExtensions.ApplyModifier<TestModifierWorld, SpeedCharacteristic>(
                target.GID, sourceB.GID, CharacteristicModifierOp.Override, 7f);

            Assert.AreEqual(7f, target.Read<CharacteristicComponent<SpeedCharacteristic>>().Value);
        }

        [Test]
        public void RemoveModifiersFromSource_RestoresValue() {
            var target = NewTargetEntity(10f);
            var sourceA = World<TestModifierWorld>.NewEntity<Default>();
            var sourceB = World<TestModifierWorld>.NewEntity<Default>();

            CharacteristicModifierExtensions.ApplyModifier<TestModifierWorld, SpeedCharacteristic>(
                target.GID, sourceA.GID, CharacteristicModifierOp.Add, 5f);
            CharacteristicModifierExtensions.ApplyModifier<TestModifierWorld, SpeedCharacteristic>(
                target.GID, sourceB.GID, CharacteristicModifierOp.Add, 3f);

            var removed = CharacteristicModifierExtensions.RemoveModifiersFromSource<TestModifierWorld, SpeedCharacteristic>(
                target.GID, sourceA.GID);

            Assert.AreEqual(1, removed);
            Assert.AreEqual(13f, target.Read<CharacteristicComponent<SpeedCharacteristic>>().Value);
        }

        private static World<TestModifierWorld>.Entity NewTargetEntity(float baseValue) {
            var entity = World<TestModifierWorld>.NewEntity<Default>();
            entity.Set(CharacteristicComponent<SpeedCharacteristic>.Create(baseValue, 0f, 1000f));
            return entity;
        }
    }
}
