namespace UniGame.StaticEcs.Features.Tests.Movement.Astar
{
    using System.Reflection;
    using FFS.Libraries.StaticEcs;
    using FFS.Libraries.StaticEcs.Unity;
    using NUnit.Framework;
    using Pathfinding;
    using Pathfinding.Graphs.Grid;
    using UniGame.StaticEcs.Tests;
    using UnityEngine;

    [TestFixture]
    [Category("StaticEcsAstar")]
    [Explicit("Run separately when STATIC_ECS_ASTAR and its backend are available.")]
    public sealed class AstarMovementSystemTests
    {
        private GameObject _host;
        private RecordingAIPath _ai;
        private GameObject _graphHost;
        private AstarPath _backend;
        private World<TestAstarWorld>.Entity _graphEntity;
        private AstarMovementSystem<TestAstarWorld> _system;
        private StaticEcsTestWorld<TestAstarWorld> _world;

        [SetUp]
        public void SetUp()
        {
            _world = new StaticEcsTestWorld<TestAstarWorld>();
            CharacteristicTypeRegistration.Register<TestAstarWorld, SpeedCharacteristic>(
                _world.Types);
            new AstarMovementFeature<TestAstarWorld>()
                .InstallResourcesAndRegisterTypesForTest(_world);
            new SpeedFeature<TestAstarWorld>()
                .InstallResourcesAndRegisterTypesForTest(_world);
            _world.Initialize();

            _host = new GameObject("Astar movement test agent");
            _ai = _host.AddComponent<RecordingAIPath>();
            _ai.enabled = false;

            _graphHost = new GameObject("Astar movement test backend");
            _backend = _graphHost.AddComponent<AstarPath>();
            var graph = _backend.data.AddGraph(typeof(GridGraph)) as GridGraph;
            graph.SetDimensions(8, 8, 1f);
            graph.collision.collisionCheck = false;
            graph.collision.heightCheck = false;
            _backend.Scan(graph);
            _graphEntity = World<TestAstarWorld>.NewEntity<Default>();
            _graphEntity.Set(new AstarPathComponent { Backend = _backend });
            _graphEntity.Set(
                new AstarGridGraphComponent
                {
                    Graph = graph,
                    NodeCount = 64,
                    WalkableNodeCount = 64,
                }
            );
            _graphEntity.Set<AstarGraphInitializedTag>();
            _system = new AstarMovementSystem<TestAstarWorld>();
        }

        [TearDown]
        public void TearDown()
        {
            _world?.Dispose();

            if (_host != null)
                Object.DestroyImmediate(_host);
            if (_graphHost != null)
                Object.DestroyImmediate(_graphHost);
        }

        [Test]
        public void Update_StartAndResume_RequestOnePathEach()
        {
            var entity = CreateAgent(new Vector3(2f, 0f, 3f), active: true);

            _system.Update();
            _system.Update();

            Assert.AreEqual(1, _ai.SearchCount);
            Assert.IsFalse(_ai.isStopped);
            Assert.IsFalse(_ai.canSearch);
            Assert.IsTrue(
                _host
                    .GetComponent<Seeker>()
                    .graphMask.Contains(_graphEntity.Read<AstarGridGraphComponent>().Graph)
            );

            entity.Mut<MovementDestinationComponent>().IsActive = false;
            _system.Update();
            Assert.IsTrue(_ai.isStopped);

            entity.Mut<MovementDestinationComponent>().IsActive = true;
            _system.Update();

            Assert.AreEqual(2, _ai.SearchCount);
            Assert.IsFalse(_ai.isStopped);
        }

        [Test]
        public void Update_WaitsForLinkedGraphAndRequestsAfterInitialization()
        {
            var graphEntity = World<TestAstarWorld>.NewEntity<Default>();
            var entity = CreateAgent(Vector3.forward, active: true, graphEntity.GID);

            _system.Update();
            Assert.AreEqual(0, _ai.SearchCount);
            Assert.IsTrue(_ai.isStopped);
            Assert.IsFalse(_ai.canSearch);

            graphEntity.Set<AstarGraphInitializedTag>();
            graphEntity.Set(new AstarPathComponent { Backend = _backend });
            graphEntity.Set(_graphEntity.Read<AstarGridGraphComponent>());
            _system.Update();

            Assert.AreEqual(1, _ai.SearchCount);
            Assert.IsFalse(_ai.isStopped);
        }

        [Test]
        public void Update_ChangedDestination_RequestsOnceForNewValue()
        {
            var entity = CreateAgent(Vector3.right, active: true);
            _system.Update();

            entity.Mut<MovementDestinationComponent>().Destination = Vector3.forward * 4f;
            _system.Update();
            _system.Update();

            Assert.AreEqual(2, _ai.SearchCount);
            Assert.AreEqual(Vector3.forward * 4f, _ai.destination);
        }

        [Test]
        public void Update_BackendMismatch_DoesNotRequestPath()
        {
            var otherHost = new GameObject("Other Astar backend");
            try
            {
                var otherBackend = otherHost.AddComponent<AstarPath>();
                var entity = CreateAgent(Vector3.right, active: true);
                var graphEntity = _graphEntity;
                graphEntity.Mut<AstarPathComponent>().Backend = otherBackend;

                _system.Update();

                Assert.AreEqual(0, _ai.SearchCount);
                Assert.IsTrue(_ai.isStopped);
            }
            finally
            {
                Object.DestroyImmediate(otherHost);
            }
        }

        [Test]
        public void Update_StartOutsideNearestDistance_DoesNotRequestPath()
        {
            _host.transform.position = Vector3.one * 1000f;
            CreateAgent(Vector3.right, active: true);

            _system.Update();

            Assert.AreEqual(0, _ai.SearchCount);
            Assert.IsTrue(_ai.isStopped);
        }

        [Test]
        public void Update_ChangedDestinationWhilePending_IsRequestedAfterPendingCompletes()
        {
            var entity = CreateAgent(Vector3.right, active: true);
            _system.Update();

            _ai.SetPending(true);
            entity.Mut<MovementDestinationComponent>().Destination = Vector3.back * 5f;
            _system.Update();
            Assert.AreEqual(1, _ai.SearchCount);

            _ai.SetPending(false);
            _system.Update();

            Assert.AreEqual(2, _ai.SearchCount);
            Assert.AreEqual(
                Vector3.back * 5f,
                entity.Read<AstarAIComponent>().LastRequestedDestination
            );
        }

        [Test]
        public void Update_AppliesSpeedAndIgnoresNullAI()
        {
            var entity = CreateAgent(Vector3.one, active: true);
            entity.Set(CharacteristicComponent<SpeedCharacteristic>.Create(7.5f, 0f, 20f));
            var nullAgent = World<TestAstarWorld>.NewEntity<Default>();
            nullAgent.Set(
                new MovementDestinationComponent { Destination = Vector3.left, IsActive = true }
            );
            nullAgent.Set(new AstarAIComponent());

            Assert.DoesNotThrow(() => _system.Update());
            Assert.AreEqual(7.5f, _ai.maxSpeed, 0.0001f);
        }

        [Test]
        public void Converter_ReadsAstarAIFromHost()
        {
            var entity = World<TestAstarWorld>.NewEntity<Default>();
            var converter = _host.AddComponent<TestAstarMovementConverter>();
            var providerHost = new GameObject("Astar graph provider");
            var provider = providerHost.AddComponent<TestAstarEntityProvider>();
            var graphEntity = World<TestAstarWorld>.NewEntity<Default>();
            provider.EntityGid = graphEntity.GID;
            typeof(AstarMovementConverter<TestAstarWorld>)
                .GetField("_graphProvider", BindingFlags.Instance | BindingFlags.NonPublic)
                ?.SetValue(converter, provider);

            try
            {
                converter.Apply(entity, _host);
                converter.ResolveLinks(entity, _host);

                Assert.AreSame(_ai, entity.Read<AstarAIComponent>().AI);
                Assert.AreSame(
                    _host.GetComponent<Seeker>(),
                    entity.Read<AstarAIComponent>().Seeker
                );
                Assert.AreEqual(graphEntity.GID, entity.Read<AstarAIComponent>().GraphEntity);
            }
            finally
            {
                Object.DestroyImmediate(providerHost);
            }
        }

        [Test]
        public void SerializableConverter_ReadsHostAndResolvesGraphProvider()
        {
            var entity = World<TestAstarWorld>.NewEntity<Default>();
            var providerHost = new GameObject("Astar serializable graph provider");
            var provider = providerHost.AddComponent<TestAstarEntityProvider>();
            var graphEntity = World<TestAstarWorld>.NewEntity<Default>();
            provider.EntityGid = graphEntity.GID;
            var converter = new AstarMovementSerializableConverter<TestAstarWorld>
            {
                GraphProvider = provider,
            };

            try
            {
                converter.Apply(entity, _host);
                converter.ResolveLinks(entity, _host);

                Assert.AreSame(_ai, entity.Read<AstarAIComponent>().AI);
                Assert.AreSame(
                    _host.GetComponent<Seeker>(),
                    entity.Read<AstarAIComponent>().Seeker
                );
                Assert.AreEqual(graphEntity.GID, entity.Read<AstarAIComponent>().GraphEntity);
            }
            finally
            {
                Object.DestroyImmediate(providerHost);
            }
        }

        [Test]
        public void MovementOperations_CreateUpdateAndStopDestination()
        {
            var entity = World<TestAstarWorld>.NewEntity<Default>();

            MovementOperations.SetDestination<TestAstarWorld>(entity.GID, Vector3.right);
            Assert.IsTrue(MovementOperations.IsMoving<TestAstarWorld>(entity.GID));
            Assert.AreEqual(Vector3.right, entity.Read<MovementDestinationComponent>().Destination);

            MovementOperations.SetDestination<TestAstarWorld>(entity.GID, Vector3.forward);
            Assert.AreEqual(
                Vector3.forward,
                entity.Read<MovementDestinationComponent>().Destination
            );

            MovementOperations.StopMovement<TestAstarWorld>(entity.GID);
            Assert.IsFalse(MovementOperations.IsMoving<TestAstarWorld>(entity.GID));
        }

        private World<TestAstarWorld>.Entity CreateAgent(
            Vector3 destination,
            bool active,
            EntityGID graphGid = default
        )
        {
            if (graphGid.Equals(default(EntityGID)))
                graphGid = _graphEntity.GID;

            var entity = World<TestAstarWorld>.NewEntity<Default>();
            entity.Set(
                new MovementDestinationComponent { Destination = destination, IsActive = active }
            );
            entity.Set(
                new AstarAIComponent
                {
                    AI = _ai,
                    Seeker = _host.GetComponent<Seeker>(),
                    GraphEntity = graphGid,
                }
            );
            return entity;
        }
    }

    public struct TestAstarWorld : IWorldType { }

    public sealed class TestAstarMovementConverter : AstarMovementConverter<TestAstarWorld> { }

    public sealed class RecordingAIPath : AIPath
    {
        public int SearchCount { get; private set; }

        public override void SearchPath()
        {
            SearchCount++;
        }

        public void SetPending(bool value)
        {
            waitingForPathCalculation = value;
        }
    }
}
