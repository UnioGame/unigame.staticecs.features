using FFS.Libraries.StaticEcs;
using NUnit.Framework;
using Pathfinding;
using UnityEngine;

namespace unigame.staticecs.features.Tests.Movement.Astar {
    [TestFixture]
    public sealed class AstarMovementSystemTests {
        private GameObject _host;
        private RecordingAIPath _ai;
        private AstarMovementSystem<TestAstarWorld> _system;

        [SetUp]
        public void SetUp() {
            World<TestAstarWorld>.Create(WorldConfig.Default());
            new AstarMovementFeature<TestAstarWorld>().RegisterTypes(World<TestAstarWorld>.Types());
            new SpeedFeature<TestAstarWorld>().RegisterTypes(World<TestAstarWorld>.Types());
            World<TestAstarWorld>.Initialize();

            _host = new GameObject("Astar movement test agent");
            _ai = _host.AddComponent<RecordingAIPath>();
            _ai.enabled = false;
            _system = new AstarMovementSystem<TestAstarWorld>();
        }

        [TearDown]
        public void TearDown() {
            if (World<TestAstarWorld>.Status != WorldStatus.NotCreated) {
                World<TestAstarWorld>.Destroy();
            }

            if (_host != null) {
                Object.DestroyImmediate(_host);
            }
        }

        [Test]
        public void Update_StartAndResume_RequestOnePathEach() {
            var entity = CreateAgent(new Vector3(2f, 0f, 3f), active: true);

            _system.Update();
            _system.Update();

            Assert.AreEqual(1, _ai.SearchCount);
            Assert.IsFalse(_ai.isStopped);

            entity.Mut<MovementDestinationComponent>().IsActive = false;
            _system.Update();
            Assert.IsTrue(_ai.isStopped);

            entity.Mut<MovementDestinationComponent>().IsActive = true;
            _system.Update();

            Assert.AreEqual(2, _ai.SearchCount);
            Assert.IsFalse(_ai.isStopped);
        }

        [Test]
        public void Update_ChangedDestination_RequestsOnceForNewValue() {
            var entity = CreateAgent(Vector3.right, active: true);
            _system.Update();

            entity.Mut<MovementDestinationComponent>().Destination = Vector3.forward * 4f;
            _system.Update();
            _system.Update();

            Assert.AreEqual(2, _ai.SearchCount);
            Assert.AreEqual(Vector3.forward * 4f, _ai.destination);
        }

        [Test]
        public void Update_ChangedDestinationWhilePending_IsRequestedAfterPendingCompletes() {
            var entity = CreateAgent(Vector3.right, active: true);
            _system.Update();

            _ai.SetPending(true);
            entity.Mut<MovementDestinationComponent>().Destination = Vector3.back * 5f;
            _system.Update();
            Assert.AreEqual(1, _ai.SearchCount);

            _ai.SetPending(false);
            _system.Update();

            Assert.AreEqual(2, _ai.SearchCount);
            Assert.AreEqual(Vector3.back * 5f, entity.Read<AstarAIComponent>().LastRequestedDestination);
        }

        [Test]
        public void Update_AppliesSpeedAndIgnoresNullAI() {
            var entity = CreateAgent(Vector3.one, active: true);
            entity.Set(CharacteristicComponent<SpeedCharacteristic>.Create(7.5f, 0f, 20f));
            var nullAgent = World<TestAstarWorld>.NewEntity<Default>();
            nullAgent.Set(new MovementDestinationComponent { Destination = Vector3.left, IsActive = true });
            nullAgent.Set(new AstarAIComponent());

            Assert.DoesNotThrow(() => _system.Update());
            Assert.AreEqual(7.5f, _ai.maxSpeed, 0.0001f);
        }

        [Test]
        public void Converter_ReadsAstarAIFromHost() {
            var entity = World<TestAstarWorld>.NewEntity<Default>();
            var converter = _host.AddComponent<TestAstarMovementConverter>();

            converter.Apply(entity, _host);

            Assert.AreSame(_ai, entity.Read<AstarAIComponent>().AI);
        }

        [Test]
        public void MovementOperations_CreateUpdateAndStopDestination() {
            var entity = World<TestAstarWorld>.NewEntity<Default>();

            MovementOperations.SetDestination<TestAstarWorld>(entity.GID, Vector3.right);
            Assert.IsTrue(MovementOperations.IsMoving<TestAstarWorld>(entity.GID));
            Assert.AreEqual(Vector3.right, entity.Read<MovementDestinationComponent>().Destination);

            MovementOperations.SetDestination<TestAstarWorld>(entity.GID, Vector3.forward);
            Assert.AreEqual(Vector3.forward, entity.Read<MovementDestinationComponent>().Destination);

            MovementOperations.StopMovement<TestAstarWorld>(entity.GID);
            Assert.IsFalse(MovementOperations.IsMoving<TestAstarWorld>(entity.GID));
        }

        private World<TestAstarWorld>.Entity CreateAgent(Vector3 destination, bool active) {
            var entity = World<TestAstarWorld>.NewEntity<Default>();
            entity.Set(new MovementDestinationComponent { Destination = destination, IsActive = active });
            entity.Set(new AstarAIComponent { AI = _ai });
            return entity;
        }
    }

    public struct TestAstarWorld : IWorldType { }

    public sealed class TestAstarMovementConverter : AstarMovementConverter<TestAstarWorld> { }

    public sealed class RecordingAIPath : AIPath {
        public int SearchCount { get; private set; }

        public override void SearchPath() {
            SearchCount++;
        }

        public void SetPending(bool value) {
            waitingForPathCalculation = value;
        }
    }
}
