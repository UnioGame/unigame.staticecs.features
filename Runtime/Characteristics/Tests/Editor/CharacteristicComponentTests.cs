using NUnit.Framework;
using UniGame.StaticEcs.Features;

namespace UniGame.StaticEcs.Features.Tests {
    [TestFixture]
    public sealed class CharacteristicComponentTests {
        [Test]
        public void Create_Clamps_Initial_Value_To_Range() {
            var c = CharacteristicComponent<ManaCharacteristic>.Create(150f, 0f, 100f);

            Assert.AreEqual(100f, c.Value);
            Assert.AreEqual(100f, c.BaseValue);
            Assert.AreEqual(0f, c.MinValue);
            Assert.AreEqual(100f, c.MaxValue);
        }

        [Test]
        public void Create_Below_Min_Clamps_Up() {
            var c = CharacteristicComponent<ManaCharacteristic>.Create(-10f, 0f, 100f);
            Assert.AreEqual(0f, c.Value);
        }

        [Test]
        public void AddValue_Clamps_To_Max() {
            var c = CharacteristicComponent<ManaCharacteristic>.Create(80f, 0f, 100f);

            var result = c.AddValue(50f);

            Assert.AreEqual(100f, result);
            Assert.AreEqual(100f, c.Value);
        }

        [Test]
        public void AddValue_Negative_Clamps_To_Min() {
            var c = CharacteristicComponent<ManaCharacteristic>.Create(20f, 0f, 100f);

            var result = c.AddValue(-50f);

            Assert.AreEqual(0f, result);
            Assert.AreEqual(0f, c.Value);
        }

        [Test]
        public void SetLimits_Clamps_Current_When_Requested() {
            var c = CharacteristicComponent<ShieldCharacteristic>.Create(150f, 0f, 200f);

            c.SetLimits(0f, 100f, clampCurrent: true);

            Assert.AreEqual(100f, c.MaxValue);
            Assert.AreEqual(100f, c.Value);
        }

        [Test]
        public void SetLimits_Inverted_Range_Snaps_Max_To_Min() {
            var c = CharacteristicComponent<HealthCharacteristic>.Create(50f, 0f, 100f);

            c.SetLimits(50f, 10f);

            Assert.AreEqual(50f, c.MinValue);
            Assert.AreEqual(50f, c.MaxValue);
        }

        [Test]
        public void SetBaseValue_Clamps_To_Limits() {
            var c = CharacteristicComponent<SpeedCharacteristic>.Create(5f, 0f, 10f);

            c.SetBaseValue(99f);

            Assert.AreEqual(10f, c.BaseValue);
        }

        [Test]
        public void SetBaseValue_Reset_Updates_Current() {
            var c = CharacteristicComponent<SpeedCharacteristic>.Create(5f, 0f, 10f);

            c.SetBaseValue(7f, resetValue: true);

            Assert.AreEqual(7f, c.BaseValue);
            Assert.AreEqual(7f, c.Value);
        }
    }
}
