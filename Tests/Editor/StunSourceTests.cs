using FFS.Libraries.StaticEcs;
using NUnit.Framework;

namespace unigame.staticecs.features.Tests {
    public sealed class StunSourceTests {
        [SetUp]
        public void SetUp() {
            World<TestStunWorld>.Create(WorldConfig.Default());
            new ModifierBackRefFeature<TestStunWorld>().RegisterTypes(World<TestStunWorld>.Types());
            new StunFeature<TestStunWorld>().RegisterTypes(World<TestStunWorld>.Types());
            World<TestStunWorld>.Initialize();
        }

        [TearDown]
        public void TearDown() {
            if (World<TestStunWorld>.Status != WorldStatus.NotCreated) {
                World<TestStunWorld>.Destroy();
            }
        }

        [Test]
        public void TwoSources_FirstAdded_ActivatesTag() {
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
        public void RemoveSource_KeepsTagWhileOthersPresent() {
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
        public void DestroySource_AutoRemovesItsEntry() {
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
        public void Clear_RemovesAllSourcesAndTag() {
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
