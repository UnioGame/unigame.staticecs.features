namespace UniGame.StaticEcs.Features.Tests
{
    using FFS.Libraries.StaticEcs;
    using NUnit.Framework;
    using UniGame.StaticEcs.Tests;

    [TestFixture]
    public sealed class StunSourceTests
    {
        private StaticEcsTestWorld<TestStunWorld> _world;

        [SetUp]
        public void SetUp()
        {
            _world = new StaticEcsTestWorld<TestStunWorld>();
            new ModifierBackRefFeature<TestStunWorld>()
                .InstallResourcesAndRegisterTypesForTest(_world);
            new StunFeature<TestStunWorld>()
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
            var target = World<TestStunWorld>.NewEntity<Default>();
            var sourceA = World<TestStunWorld>.NewEntity<Default>();
            var sourceB = World<TestStunWorld>.NewEntity<Default>();

            StunOperations.AddSource<TestStunWorld>(target.GID, sourceA.GID);
            Assert.IsTrue(StunOperations.IsActive<TestStunWorld>(target.GID));
            Assert.AreEqual(1, StunOperations.SourceCount<TestStunWorld>(target.GID));

            StunOperations.AddSource<TestStunWorld>(target.GID, sourceB.GID);
            Assert.AreEqual(2, StunOperations.SourceCount<TestStunWorld>(target.GID));
            Assert.IsTrue(StunOperations.IsActive<TestStunWorld>(target.GID));
        }

        [Test]
        public void RemoveSource_KeepsTagWhileOthersPresent()
        {
            var target = World<TestStunWorld>.NewEntity<Default>();
            var sourceA = World<TestStunWorld>.NewEntity<Default>();
            var sourceB = World<TestStunWorld>.NewEntity<Default>();

            StunOperations.AddSource<TestStunWorld>(target.GID, sourceA.GID);
            StunOperations.AddSource<TestStunWorld>(target.GID, sourceB.GID);

            StunOperations.RemoveSource<TestStunWorld>(target.GID, sourceA.GID);

            Assert.IsTrue(StunOperations.IsActive<TestStunWorld>(target.GID));
            Assert.AreEqual(1, StunOperations.SourceCount<TestStunWorld>(target.GID));

            StunOperations.RemoveSource<TestStunWorld>(target.GID, sourceB.GID);

            Assert.IsFalse(StunOperations.IsActive<TestStunWorld>(target.GID));
            Assert.AreEqual(0, StunOperations.SourceCount<TestStunWorld>(target.GID));
        }

        [Test]
        public void DestroySource_AutoRemovesItsEntry()
        {
            var target = World<TestStunWorld>.NewEntity<Default>();
            var sourceA = World<TestStunWorld>.NewEntity<Default>();
            var sourceB = World<TestStunWorld>.NewEntity<Default>();

            StunOperations.AddSource<TestStunWorld>(target.GID, sourceA.GID);
            StunOperations.AddSource<TestStunWorld>(target.GID, sourceB.GID);

            sourceA.Destroy();

            Assert.AreEqual(1, StunOperations.SourceCount<TestStunWorld>(target.GID));
            Assert.IsTrue(StunOperations.IsActive<TestStunWorld>(target.GID));

            sourceB.Destroy();

            Assert.AreEqual(0, StunOperations.SourceCount<TestStunWorld>(target.GID));
            Assert.IsFalse(StunOperations.IsActive<TestStunWorld>(target.GID));
        }

        [Test]
        public void Clear_RemovesAllSourcesAndTag()
        {
            var target = World<TestStunWorld>.NewEntity<Default>();
            var sourceA = World<TestStunWorld>.NewEntity<Default>();
            var sourceB = World<TestStunWorld>.NewEntity<Default>();

            StunOperations.AddSource<TestStunWorld>(target.GID, sourceA.GID);
            StunOperations.AddSource<TestStunWorld>(target.GID, sourceB.GID);

            Assert.IsTrue(StunOperations.Clear<TestStunWorld>(target.GID));
            Assert.IsFalse(StunOperations.IsActive<TestStunWorld>(target.GID));
            Assert.AreEqual(0, StunOperations.SourceCount<TestStunWorld>(target.GID));
        }
    }
}
