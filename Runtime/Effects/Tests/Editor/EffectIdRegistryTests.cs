namespace UniGame.StaticEcs.Features.Tests
{
    using NUnit.Framework;

    [TestFixture]
    public sealed class EffectIdRegistryTests
    {
        [Test]
        public void Register_AssignsStableIds_AndReverseLookupYieldsType()
        {
            var registry = new EffectIdRegistry();
            var idA = registry.Register<TestEffectMarker>();
            var idB = registry.Register<TestEffectMarkerB>();

            Assert.AreNotEqual(idA, idB);
            Assert.AreEqual(idA, registry.Register<TestEffectMarker>());

            Assert.IsTrue(registry.TryGetType(idA, out var typeA));
            Assert.AreEqual(typeof(TestEffectMarker), typeA);
        }

        [Test]
        public void GetTypeName_ReturnsTypeName_OrFallback()
        {
            var registry = new EffectIdRegistry();
            var id = registry.Register<TestEffectMarker>();

            Assert.AreEqual(nameof(TestEffectMarker), registry.GetTypeName(id));
            Assert.AreEqual("Effect#999", registry.GetTypeName(new EffectId(999)));
        }
    }

    [TestFixture]
    public sealed class EffectFlagOfTests
    {
        [Test]
        public void Resolve_ReadsAttribute_FromOpenGenericDefinition()
        {
            Assert.AreEqual(EffectFlag.Reserved0, EffectFlagOf<TestEffectMarker>.Value);
            Assert.AreEqual(EffectFlag.Reserved1, EffectFlagOf<TestEffectMarkerB>.Value);
        }

        [Test]
        public void Resolve_ProductionEffects_HaveSingleBitFlags()
        {
            AssertSingleBit(EffectFlagOf<HealOverTimeEffect>.Value);
            AssertSingleBit(EffectFlagOf<StunEffect>.Value);
            AssertSingleBit(EffectFlagOf<ModificationEffect<SpeedCharacteristic>>.Value);
        }

        private static void AssertSingleBit(EffectFlag flag)
        {
            var bits = (ulong)flag;
            Assert.AreNotEqual(0ul, bits);
            Assert.AreEqual(0ul, bits & (bits - 1), flag.ToString());
        }
    }
}
