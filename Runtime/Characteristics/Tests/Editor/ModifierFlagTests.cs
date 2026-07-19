using System;
using FFS.Libraries.StaticEcs;
using NUnit.Framework;

namespace UniGame.StaticEcs.Features.Tests {
    [TestFixture]
    public sealed class ModifierFlagTests {
        [SetUp]
        public void SetUp() {
            World<TestModifierWorld>.Create(WorldConfig.Default());
            new ModifierBackRefFeature<TestModifierWorld>().RegisterTypes(World<TestModifierWorld>.Types());
            new CharacteristicFeature<TestModifierWorld, SpeedCharacteristic>().RegisterTypes(World<TestModifierWorld>.Types());
            new CharacteristicFeature<TestModifierWorld, ManaCharacteristic>().RegisterTypes(World<TestModifierWorld>.Types());
            World<TestModifierWorld>.Initialize();
        }

        [TearDown]
        public void TearDown() {
            if (World<TestModifierWorld>.Status != WorldStatus.NotCreated) {
                World<TestModifierWorld>.Destroy();
            }
        }

        [Test]
        public void BackRef_DeduplicatesPerTarget_AcrossStats() {
            var target = NewTarget();
            var source = World<TestModifierWorld>.NewEntity<Default>();

            CharacteristicModifierExtensions.ApplyModifier<TestModifierWorld, SpeedCharacteristic>(
                target.GID, source.GID, CharacteristicModifierOp.Add, 5f);
            CharacteristicModifierExtensions.ApplyModifier<TestModifierWorld, ManaCharacteristic>(
                target.GID, source.GID, CharacteristicModifierOp.Add, 7f);

            ref var refs = ref source.Ref<World<TestModifierWorld>.Multi<ModifierBackRef>>();
            Assert.AreEqual(1, refs.Length);
            Assert.AreEqual(CharacteristicFlag.Speed | CharacteristicFlag.Mana, refs[0].StatMask);
        }

        [Test]
        public void BackRef_TwoTargets_TwoEntries() {
            var targetA = NewTarget();
            var targetB = NewTarget();
            var source = World<TestModifierWorld>.NewEntity<Default>();

            CharacteristicModifierExtensions.ApplyModifier<TestModifierWorld, SpeedCharacteristic>(
                targetA.GID, source.GID, CharacteristicModifierOp.Add, 5f);
            CharacteristicModifierExtensions.ApplyModifier<TestModifierWorld, SpeedCharacteristic>(
                targetB.GID, source.GID, CharacteristicModifierOp.Add, 7f);

            ref var refs = ref source.Ref<World<TestModifierWorld>.Multi<ModifierBackRef>>();
            Assert.AreEqual(2, refs.Length);
            Assert.AreEqual(CharacteristicFlag.Speed, refs[0].StatMask);
            Assert.AreEqual(CharacteristicFlag.Speed, refs[1].StatMask);
        }

        [Test]
        public void DestroySource_CleansBothStats_FromSingleEntry() {
            var target = NewTarget();
            var source = World<TestModifierWorld>.NewEntity<Default>();

            CharacteristicModifierExtensions.ApplyModifier<TestModifierWorld, SpeedCharacteristic>(
                target.GID, source.GID, CharacteristicModifierOp.Add, 5f);
            CharacteristicModifierExtensions.ApplyModifier<TestModifierWorld, ManaCharacteristic>(
                target.GID, source.GID, CharacteristicModifierOp.Add, 7f);

            Assert.AreEqual(15f, target.Read<CharacteristicComponent<SpeedCharacteristic>>().Value);
            Assert.AreEqual(57f, target.Read<CharacteristicComponent<ManaCharacteristic>>().Value);

            source.Destroy();

            Assert.AreEqual(10f, target.Read<CharacteristicComponent<SpeedCharacteristic>>().Value);
            Assert.AreEqual(50f, target.Read<CharacteristicComponent<ManaCharacteristic>>().Value);
        }

        [Test]
        public void MissingAttribute_ResolveThrows() {
            Assert.Throws<TypeInitializationException>(() => {
                _ = CharacteristicFlagOf<UnflaggedMarker>.Value;
            });
        }

        private struct UnflaggedMarker : ICharacteristicType { }

        private static World<TestModifierWorld>.Entity NewTarget() {
            var entity = World<TestModifierWorld>.NewEntity<Default>();
            entity.Set(CharacteristicComponent<SpeedCharacteristic>.Create(10f, 0f, 1000f));
            entity.Set(CharacteristicComponent<ManaCharacteristic>.Create(50f, 0f, 1000f));
            return entity;
        }
    }
}
