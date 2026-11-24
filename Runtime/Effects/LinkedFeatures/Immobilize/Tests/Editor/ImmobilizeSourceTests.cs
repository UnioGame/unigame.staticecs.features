namespace UniGame.StaticEcs.Features.Tests
{
    using FFS.Libraries.StaticEcs;
    using NUnit.Framework;
    using UniGame.StaticEcs.Tests;

    [TestFixture]
    public sealed class ImmobilizeSourceTests
    {
        private StaticEcsTestWorld<TestImmobilizeWorld> _world;

        [SetUp]
        public void SetUp()
        {
            _world = new StaticEcsTestWorld<TestImmobilizeWorld>();
            new ModifierBackRefFeature<TestImmobilizeWorld>()
                .InstallResourcesAndRegisterTypesForTest(_world);
            new ImmobilizeFeature<TestImmobilizeWorld>()
                .InstallResourcesAndRegisterTypesForTest(_world);
            _world.Initialize();
        }

        [TearDown]
        public void TearDown()
        {
            _world?.Dispose();
        }

        [Test]
        public void TwoSources_FirstAdded_ActivatesTag()
        {
            var target = World<TestImmobilizeWorld>.NewEntity<Default>();
            var sourceA = World<TestImmobilizeWorld>.NewEntity<Default>();
            var sourceB = World<TestImmobilizeWorld>.NewEntity<Default>();

            ImmobilizeOperations.AddSource<TestImmobilizeWorld>(target.GID, sourceA.GID);
            Assert.IsTrue(ImmobilizeOperations.IsActive<TestImmobilizeWorld>(target.GID));
            Assert.AreEqual(1, ImmobilizeOperations.SourceCount<TestImmobilizeWorld>(target.GID));

            ImmobilizeOperations.AddSource<TestImmobilizeWorld>(target.GID, sourceB.GID);
            Assert.AreEqual(2, ImmobilizeOperations.SourceCount<TestImmobilizeWorld>(target.GID));
            Assert.IsTrue(ImmobilizeOperations.IsActive<TestImmobilizeWorld>(target.GID));
        }

        [Test]
        public void RemoveSource_KeepsTagWhileOthersPresent()
        {
            var target = World<TestImmobilizeWorld>.NewEntity<Default>();
            var sourceA = World<TestImmobilizeWorld>.NewEntity<Default>();
            var sourceB = World<TestImmobilizeWorld>.NewEntity<Default>();

            ImmobilizeOperations.AddSource<TestImmobilizeWorld>(target.GID, sourceA.GID);
            ImmobilizeOperations.AddSource<TestImmobilizeWorld>(target.GID, sourceB.GID);

            ImmobilizeOperations.RemoveSource<TestImmobilizeWorld>(target.GID, sourceA.GID);

            Assert.IsTrue(ImmobilizeOperations.IsActive<TestImmobilizeWorld>(target.GID));
            Assert.AreEqual(1, ImmobilizeOperations.SourceCount<TestImmobilizeWorld>(target.GID));

            ImmobilizeOperations.RemoveSource<TestImmobilizeWorld>(target.GID, sourceB.GID);

            Assert.IsFalse(ImmobilizeOperations.IsActive<TestImmobilizeWorld>(target.GID));
            Assert.AreEqual(0, ImmobilizeOperations.SourceCount<TestImmobilizeWorld>(target.GID));
        }

        [Test]
        public void DestroySource_AutoRemovesItsEntry()
        {
            var target = World<TestImmobilizeWorld>.NewEntity<Default>();
            var sourceA = World<TestImmobilizeWorld>.NewEntity<Default>();
            var sourceB = World<TestImmobilizeWorld>.NewEntity<Default>();

            ImmobilizeOperations.AddSource<TestImmobilizeWorld>(target.GID, sourceA.GID);
            ImmobilizeOperations.AddSource<TestImmobilizeWorld>(target.GID, sourceB.GID);

            sourceA.Destroy();

            Assert.AreEqual(1, ImmobilizeOperations.SourceCount<TestImmobilizeWorld>(target.GID));
            Assert.IsTrue(ImmobilizeOperations.IsActive<TestImmobilizeWorld>(target.GID));

            sourceB.Destroy();

            Assert.AreEqual(0, ImmobilizeOperations.SourceCount<TestImmobilizeWorld>(target.GID));
            Assert.IsFalse(ImmobilizeOperations.IsActive<TestImmobilizeWorld>(target.GID));
        }

        [Test]
        public void Clear_RemovesAllSourcesAndTag()
        {
            var target = World<TestImmobilizeWorld>.NewEntity<Default>();
            var sourceA = World<TestImmobilizeWorld>.NewEntity<Default>();
            var sourceB = World<TestImmobilizeWorld>.NewEntity<Default>();

            ImmobilizeOperations.AddSource<TestImmobilizeWorld>(target.GID, sourceA.GID);
            ImmobilizeOperations.AddSource<TestImmobilizeWorld>(target.GID, sourceB.GID);

            Assert.IsTrue(ImmobilizeOperations.Clear<TestImmobilizeWorld>(target.GID));
            Assert.IsFalse(ImmobilizeOperations.IsActive<TestImmobilizeWorld>(target.GID));
            Assert.AreEqual(0, ImmobilizeOperations.SourceCount<TestImmobilizeWorld>(target.GID));
        }
    }
}
