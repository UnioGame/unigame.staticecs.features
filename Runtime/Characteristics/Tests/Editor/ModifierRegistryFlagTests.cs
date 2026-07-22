namespace UniGame.StaticEcs.Features.Tests
{
    using System;
    using FFS.Libraries.StaticEcs;
    using NUnit.Framework;
    using UniGame.StaticEcs.Modifiers;

    [TestFixture]
    public sealed class ModifierRegistryFlagTests
    {
        [Test]
        public void Register_RejectsZero()
        {
            var registry = new ModifierRegistry();
            Assert.Throws<ArgumentException>(() => registry.Register(0UL, NoopCleanup));
        }

        [Test]
        public void Register_RejectsNonPowerOfTwo()
        {
            var registry = new ModifierRegistry();
            Assert.Throws<ArgumentException>(() => registry.Register(0b11UL, NoopCleanup));
        }

        [Test]
        public void Register_RejectsNullCleanup()
        {
            var registry = new ModifierRegistry();
            Assert.Throws<ArgumentNullException>(() => registry.Register(1UL << 5, null));
        }

        [Test]
        public void InvokeMask_VisitsEachSetBit_Once()
        {
            var registry = new ModifierRegistry();
            var aHits = 0;
            var bHits = 0;
            var flagA = (ulong)CharacteristicFlag.Health;
            var flagB = (ulong)CharacteristicFlag.Speed;

            registry.Register(flagA, (_, _) => aHits++);
            registry.Register(flagB, (_, _) => bHits++);

            registry.InvokeMask(flagA | flagB, default, default);

            Assert.AreEqual(1, aHits);
            Assert.AreEqual(1, bHits);
        }

        [Test]
        public void Invoke_NoopForUnregisteredFlag()
        {
            var registry = new ModifierRegistry();
            Assert.DoesNotThrow(() => registry.Invoke(1UL << 10, default, default));
        }

        private static void NoopCleanup(EntityGID source, EntityGID target) { }
    }
}
