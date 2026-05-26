using System;
using FFS.Libraries.StaticEcs;
using NUnit.Framework;
using UnityEngine;
using unigame.staticecs.unity;

namespace unigame.staticecs.features.Tests {
    [TestFixture]
    public sealed class KdTreeTargetIndexTests {
        private GameObject[] _hosts;

        [SetUp]
        public void SetUp() {
            World<TestTargetIndexWorld>.Create(WorldConfig.Default());
            new TargetSelectionFeature<TestTargetIndexWorld>(registerRebuildSystem: false)
                .RegisterTypes(World<TestTargetIndexWorld>.Types());
            World<TestTargetIndexWorld>.Types()
                .Component<TransformBindingComponent>();

            World<TestTargetIndexWorld>.Initialize();
        }

        [TearDown]
        public void TearDown() {
            if (_hosts != null) {
                foreach (var host in _hosts) {
                    if (host != null) {
                        UnityEngine.Object.DestroyImmediate(host);
                    }
                }
                _hosts = null;
            }
            if (World<TestTargetIndexWorld>.Status != WorldStatus.NotCreated) {
                World<TestTargetIndexWorld>.Destroy();
            }
        }

        private EntityGID Spawn(Vector3 position) {
            var host = new GameObject("kd-target");
            host.transform.position = position;
            var entity = World<TestTargetIndexWorld>.NewEntity<Default>();
            entity.Set<TargetableTag>();
            entity.Set(new TransformBindingComponent { Transform = host.transform });

            Array.Resize(ref _hosts, (_hosts?.Length ?? 0) + 1);
            _hosts[_hosts.Length - 1] = host;
            return entity.GID;
        }

        [Test]
        public void FillSphere_ReturnsZero_WhenIndexEmpty() {
            var index = World<TestTargetIndexWorld>.GetResource<ITargetIndex<TestTargetIndexWorld>>();
            index.Rebuild();

            Span<EntityGID> buffer = stackalloc EntityGID[4];
            Assert.AreEqual(0, index.FillSphere(Vector3.zero, 5f, buffer));
        }

        [Test]
        public void FillSphere_FindsAllPointsInsideRadius() {
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
            for (var i = 0; i < count; i++) {
                found.Add(buffer[i]);
            }
            Assert.IsTrue(found.Contains(center));
            Assert.IsTrue(found.Contains(near));
            Assert.IsFalse(found.Contains(far));
        }

        [Test]
        public void FillSphere_RespectsBufferCapacity() {
            for (var i = 0; i < 8; i++) {
                Spawn(new Vector3(i * 0.1f, 0, 0));
            }
            var index = World<TestTargetIndexWorld>.GetResource<ITargetIndex<TestTargetIndexWorld>>();
            index.Rebuild();

            Span<EntityGID> buffer = stackalloc EntityGID[3];
            var count = index.FillSphere(Vector3.zero, 100f, buffer);
            Assert.AreEqual(3, count);
        }

        [Test]
        public void Rebuild_ReflectsLatestPositions() {
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
