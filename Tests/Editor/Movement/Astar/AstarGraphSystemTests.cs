using System.Reflection;
using System.Text.RegularExpressions;
using FFS.Libraries.StaticEcs;
using NUnit.Framework;
using Pathfinding;
using Pathfinding.Graphs.Grid;
using unigame.staticecs.unity;
using UnityEngine;
using UnityEngine.TestTools;

namespace unigame.staticecs.features.Tests.Movement.Astar {
    [TestFixture]
    public sealed class AstarGraphSystemTests {
        private GameObject _graphHost;
        private AstarPath _backend;
        private AstarGraphSystem<TestAstarWorld> _system;

        [SetUp]
        public void SetUp() {
            World<TestAstarWorld>.Create(WorldConfig.Default());
            new AstarMovementFeature<TestAstarWorld>().RegisterTypes(World<TestAstarWorld>.Types());
            World<TestAstarWorld>.Initialize();

            _graphHost = new GameObject("Astar graph test backend");
            _backend = _graphHost.AddComponent<AstarPath>();
            _system = new AstarGraphSystem<TestAstarWorld>();
        }

        [TearDown]
        public void TearDown() {
            if (World<TestAstarWorld>.Status != WorldStatus.NotCreated) {
                World<TestAstarWorld>.Destroy();
            }

            if (_graphHost != null) {
                Object.DestroyImmediate(_graphHost);
            }
        }

        [Test]
        public void Update_CreatesAndScansGraphOnlyOnce() {
            var entity = CreateGraphEntity();

            _system.Update();
            var firstGraph = entity.Read<AstarGridGraphRuntimeComponent>().Graph;
            _system.Update();

            Assert.IsNotNull(firstGraph);
            Assert.IsTrue(entity.Has<AstarGraphInitializedTag>());
            Assert.Greater(entity.Read<AstarGridGraphRuntimeComponent>().NodeCount, 0);
            Assert.Greater(entity.Read<AstarGridGraphRuntimeComponent>().WalkableNodeCount, 0);
            Assert.AreSame(firstGraph, entity.Read<AstarGridGraphRuntimeComponent>().Graph);
            Assert.AreEqual(1, _backend.data.graphs.Length);
        }

        [Test]
        public void Update_FullyBlockedGraphIsMarkedFailedAndNotRecreated() {
            var blocker = GameObject.CreatePrimitive(PrimitiveType.Cube);
            try {
                blocker.transform.position = new Vector3(0f, 0.5f, 0f);
                blocker.transform.localScale = new Vector3(20f, 2f, 20f);
                Physics.SyncTransforms();
                var entity = CreateGraphEntity();

                LogAssert.Expect(LogType.Error, new Regex("Grid scan is unusable: registered=True, scanned=True, nodes=.*walkable=0"));
                _system.Update();
                var graph = entity.Read<AstarGridGraphRuntimeComponent>().Graph;
                _system.Update();

                Assert.IsFalse(entity.Has<AstarGraphInitializedTag>());
                Assert.IsTrue(entity.Has<AstarGraphInitializationFailedTag>());
                Assert.AreEqual(0, entity.Read<AstarGridGraphRuntimeComponent>().WalkableNodeCount);
                Assert.AreSame(graph, entity.Read<AstarGridGraphRuntimeComponent>().Graph);
            }
            finally {
                Object.DestroyImmediate(blocker);
            }
        }

        [Test]
        public void Update_MissingBackendWaitsWithoutException() {
            var entity = World<TestAstarWorld>.NewEntity<Default>();
            entity.Set(new AstarPathComponent());
            entity.Set(DefaultConfig());

            Assert.DoesNotThrow(() => _system.Update());
            Assert.IsFalse(entity.Has<AstarGraphInitializedTag>());
        }

        [Test]
        public void Update_NonActiveBackendIsRejectedOnce() {
            var otherHost = new GameObject("Inactive Astar backend");
            try {
                var otherBackend = otherHost.AddComponent<AstarPath>();
                var entity = World<TestAstarWorld>.NewEntity<Default>();
                entity.Set(new AstarPathComponent { Backend = otherBackend });
                entity.Set(DefaultConfig());

                LogAssert.Expect(LogType.Error, new Regex("is not AstarPath.active"));
                _system.Update();
                _system.Update();

                Assert.IsTrue(entity.Has<AstarGraphInitializationFailedTag>());
                Assert.IsFalse(entity.Has<AstarGraphInitializedTag>());
            }
            finally {
                Object.DestroyImmediate(otherHost);
            }
        }

        [Test]
        public void Destroy_RemovesOwnedGraphAndPreservesForeignGraph() {
            var foreign = _backend.data.AddGraph(typeof(PointGraph));
            var entity = CreateGraphEntity();
            _system.Update();

            var owned = entity.Read<AstarGridGraphRuntimeComponent>().Graph;
            _system.Destroy();

            CollectionAssert.Contains(_backend.data.graphs, foreign);
            CollectionAssert.DoesNotContain(_backend.data.graphs, owned);
        }

        [Test]
        public void GraphConverter_CreatesBackendAndConfigurationComponents() {
            var entity = World<TestAstarWorld>.NewEntity<Default>();
            var converter = _graphHost.AddComponent<TestAstarGridGraphConverter>();

            converter.Apply(entity, _graphHost);

            Assert.AreSame(_backend, entity.Read<AstarPathComponent>().Backend);
            Assert.AreEqual(40, entity.Read<AstarGridGraphConfigComponent>().Width);
            Assert.AreEqual(0.5f, entity.Read<AstarGridGraphConfigComponent>().NodeSize);
        }

        [Test]
        public void ObstacleConverter_ResolvesGraphProviderGid() {
            var obstacleHost = new GameObject("Astar graph test obstacle");
            var providerHost = new GameObject("Astar graph test provider");
            try {
                var collider = obstacleHost.AddComponent<BoxCollider>();
                var converter = obstacleHost.AddComponent<TestAstarObstacleConverter>();
                var provider = providerHost.AddComponent<TestAstarEntityProvider>();
                var graphEntity = World<TestAstarWorld>.NewEntity<Default>();
                provider.EntityGid = graphEntity.GID;

                var field = typeof(AstarObstacleConverter<TestAstarWorld>).GetField(
                    "_graphProvider",
                    BindingFlags.Instance | BindingFlags.NonPublic);
                field.SetValue(converter, provider);

                var obstacleEntity = World<TestAstarWorld>.NewEntity<Default>();
                converter.Apply(obstacleEntity, obstacleHost);
                converter.ResolveLinks(obstacleEntity, obstacleHost);

                ref readonly var obstacle = ref obstacleEntity.Read<AstarObstacleComponent>();
                Assert.AreSame(collider, obstacle.Collider);
                Assert.AreEqual(graphEntity.GID, obstacle.GraphEntity);
            }
            finally {
                Object.DestroyImmediate(obstacleHost);
                Object.DestroyImmediate(providerHost);
            }
        }

        [Test]
        public void Update_TracksObstacleTransformAndActiveState() {
            var graphEntity = CreateGraphEntity();
            _system.Update();

            var obstacleHost = GameObject.CreatePrimitive(PrimitiveType.Cube);
            try {
                var collider = obstacleHost.GetComponent<Collider>();
                var obstacleEntity = World<TestAstarWorld>.NewEntity<Default>();
                obstacleEntity.Set(new AstarObstacleComponent {
                    GraphEntity = graphEntity.GID,
                    Collider = collider,
                });

                _system.Update();
                var initialBounds = obstacleEntity.Read<AstarObstacleComponent>().LastBounds;

                obstacleHost.transform.position = Vector3.right * 3f;
                _system.Update();

                ref readonly var moved = ref obstacleEntity.Read<AstarObstacleComponent>();
                Assert.IsTrue(moved.HasSnapshot);
                Assert.IsTrue(moved.WasActive);
                Assert.AreNotEqual(initialBounds.center, moved.LastBounds.center);

                collider.enabled = false;
                _system.Update();
                Assert.IsFalse(obstacleEntity.Read<AstarObstacleComponent>().WasActive);
            }
            finally {
                Object.DestroyImmediate(obstacleHost);
            }
        }

        [Test]
        public void DeleteObstacle_DisablesColliderBeforeClearingFootprint() {
            var graphEntity = CreateGraphEntity();
            _system.Update();

            var obstacleHost = GameObject.CreatePrimitive(PrimitiveType.Cube);
            try {
                var collider = obstacleHost.GetComponent<Collider>();
                var obstacleEntity = World<TestAstarWorld>.NewEntity<Default>();
                obstacleEntity.Set(new AstarObstacleComponent {
                    GraphEntity = graphEntity.GID,
                    Collider = collider,
                    LastBounds = collider.bounds,
                    LastLocalToWorld = obstacleHost.transform.localToWorldMatrix,
                    HasSnapshot = true,
                    WasActive = true,
                });

                obstacleEntity.Destroy();

                Assert.IsFalse(collider.enabled);
            }
            finally {
                Object.DestroyImmediate(obstacleHost);
            }
        }

        private World<TestAstarWorld>.Entity CreateGraphEntity() {
            var entity = World<TestAstarWorld>.NewEntity<Default>();
            entity.Set(new AstarPathComponent { Backend = _backend });
            entity.Set(DefaultConfig());
            return entity;
        }

        private static AstarGridGraphConfigComponent DefaultConfig() {
            return new AstarGridGraphConfigComponent {
                Width = 8,
                Depth = 8,
                NodeSize = 1f,
                ObstacleMask = ~0,
                AgentDiameter = 1f,
                AgentHeight = 1f,
                FlushGraphUpdates = true,
            };
        }
    }

    public sealed class TestAstarGridGraphConverter : AstarGridGraphConverter<TestAstarWorld> { }

    public sealed class TestAstarObstacleConverter : AstarObstacleConverter<TestAstarWorld> { }

    public sealed class TestAstarEntityProvider : EcsEntityProvider<TestAstarWorld> { }
}
