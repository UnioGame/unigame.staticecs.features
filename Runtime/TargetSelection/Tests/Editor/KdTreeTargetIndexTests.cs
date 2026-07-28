namespace UniGame.StaticEcs.Features.Tests
{
    using System;
    using FFS.Libraries.StaticEcs;
    using NUnit.Framework;
    using UniGame.StaticEcs.Tests;
    using UniGame.StaticEcs.Unity;
    using UnityEngine;

    [TestFixture]
    public sealed class KdTreeTargetIndexTests
    {
        private GameObject[] _hosts;
        private StaticEcsTestWorld<TestTargetIndexWorld> _world;

        [SetUp]
        public void SetUp()
        {
            _world = new StaticEcsTestWorld<TestTargetIndexWorld>();
            new TargetSelectionFeature<TestTargetIndexWorld>()
                .InstallResourcesAndRegisterTypesForTest(_world);
            _world.Types.Component<TransformComponent>();

            _world.Initialize();
        }

        [TearDown]
        public void TearDown()
        {
            if (_hosts != null)
            {
                foreach (var host in _hosts)
                {
                    if (host != null)
                        UnityEngine.Object.DestroyImmediate(host);
                }
                _hosts = null;
            }
            _world?.Dispose();
        }

        private EntityGID Spawn(Vector3 position)
        {
            var host = new GameObject("kd-target");
            host.transform.position = position;
            var entity = World<TestTargetIndexWorld>.NewEntity<Default>();
            entity.Set<TargetableTag>();
            entity.Set(new TransformComponent { Transform = host.transform });

            Array.Resize(ref _hosts, (_hosts?.Length ?? 0) + 1);
            _hosts[_hosts.Length - 1] = host;
            return entity.GID;
        }

        [Test]
        public void FillSphere_ReturnsZero_WhenIndexEmpty()
        {
            var index = World<TestTargetIndexWorld>.GetResource<ITargetIndex<TestTargetIndexWorld>>();
            index.Rebuild();

            Span<EntityGID> buffer = stackalloc EntityGID[4];
            Assert.AreEqual(0, index.FillSphere(Vector3.zero, 5f, buffer));
        }

        [Test]
        public void FillSphere_FindsAllPointsInsideRadius()
        {
            var center = Spawn(new Vector3(0, 0, 0));
            var near = Spawn(new Vector3(1, 0, 0));
            var far = Spawn(new Vector3(10, 0, 0));

            var index = World<TestTargetIndexWorld>.GetResource<ITargetIndex<TestTargetIndexWorld>>();
            index.Rebuild();
            Assert.AreEqual(3, index.Count);

            Span<EntityGID> buffer = stackalloc EntityGID[8];
            var count = index.FillSphere(Vector3.zero, 2f, buffer);
            Assert.AreEqual(2, count);

            var found = new System.Collections.Generic.HashSet<EntityGID>();
            for (var i = 0; i < count; i++)
            {
                found.Add(buffer[i]);
            }
            Assert.IsTrue(found.Contains(center));
            Assert.IsTrue(found.Contains(near));
            Assert.IsFalse(found.Contains(far));
        }

        [Test]
        public void FillSphere_RespectsBufferCapacity()
        {
            for (var i = 0; i < 8; i++)
            {
                Spawn(new Vector3(i * 0.1f, 0, 0));
            }
            var index = World<TestTargetIndexWorld>.GetResource<ITargetIndex<TestTargetIndexWorld>>();
            index.Rebuild();

            Span<EntityGID> buffer = stackalloc EntityGID[3];
            var count = index.FillSphere(Vector3.zero, 100f, buffer);
            Assert.AreEqual(3, count);
        }

        [Test]
        public void FillNearestSphere_SupportsLargeCallerBuffer()
        {
            var expected = Spawn(Vector3.zero);
            var index = World<TestTargetIndexWorld>.GetResource<ITargetIndex<TestTargetIndexWorld>>();
            index.Rebuild();

            var buffer = new EntityGID[1024];
            var count = index.FillNearestSphere(Vector3.zero, 1f, buffer);

            Assert.AreEqual(1, count);
            Assert.AreEqual(expected, buffer[0]);
        }

        [Test]
        public void FillNearestSphere_ReturnsNearestTargetsInStableOrder()
        {
            var farther = Spawn(new Vector3(3f, 0f, 0f));
            var nearest = Spawn(new Vector3(1f, 0f, 0f));
            var middle = Spawn(new Vector3(2f, 0f, 0f));
            var index = World<TestTargetIndexWorld>.GetResource<ITargetIndex<TestTargetIndexWorld>>();
            index.Rebuild();

            Span<EntityGID> buffer = stackalloc EntityGID[2];
            var count = index.FillNearestSphere(Vector3.zero, 10f, buffer);

            Assert.AreEqual(2, count);
            Assert.AreEqual(nearest, buffer[0]);
            Assert.AreEqual(middle, buffer[1]);
            Assert.AreNotEqual(farther, buffer[1]);
        }

        [Test]
        public void FillNearestSphere_ExcludesRequestedEntityBeforeBounding()
        {
            var excluded = Spawn(Vector3.zero);
            var first = Spawn(Vector3.right);
            var second = Spawn(Vector3.right * 2f);
            var index = World<TestTargetIndexWorld>.GetResource<ITargetIndex<TestTargetIndexWorld>>();
            index.Rebuild();

            Span<EntityGID> buffer = stackalloc EntityGID[2];
            var count = index.FillNearestSphere(Vector3.zero, 10f, buffer, excluded);

            Assert.AreEqual(2, count);
            Assert.AreEqual(first, buffer[0]);
            Assert.AreEqual(second, buffer[1]);
        }

        [Test]
        public void FillNearestSphere_UsesEntityIdAsEqualDistanceTieBreak()
        {
            var first = Spawn(Vector3.left);
            var second = Spawn(Vector3.right);
            var index = World<TestTargetIndexWorld>.GetResource<ITargetIndex<TestTargetIndexWorld>>();
            index.Rebuild();

            Span<EntityGID> buffer = stackalloc EntityGID[2];
            var count = index.FillNearestSphere(Vector3.zero, 10f, buffer);

            Assert.AreEqual(2, count);
            var expectedFirst = first.Raw < second.Raw ? first : second;
            var expectedSecond = first.Raw < second.Raw ? second : first;
            Assert.AreEqual(expectedFirst, buffer[0]);
            Assert.AreEqual(expectedSecond, buffer[1]);
        }

        [Test]
        public void Rebuild_ReflectsLatestPositions()
        {
            var moving = Spawn(new Vector3(10, 0, 0));
            var index = World<TestTargetIndexWorld>.GetResource<ITargetIndex<TestTargetIndexWorld>>();
            index.Rebuild();

            Span<EntityGID> buffer = stackalloc EntityGID[4];
            Assert.AreEqual(0, index.FillSphere(Vector3.zero, 1f, buffer));

            _hosts[0].transform.position = Vector3.zero;
            index.Rebuild();
            Assert.AreEqual(1, index.FillSphere(Vector3.zero, 1f, buffer));
            Assert.AreEqual(moving, buffer[0]);
        }
    }
}
