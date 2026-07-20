using FFS.Libraries.StaticEcs;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.AI;

namespace UniGame.StaticEcs.Features.Tests.Movement
{
    public sealed class NavMeshMovementSerializableConverterTests
    {
        private GameObject _host;

        [SetUp]
        public void SetUp()
        {
            World<TestNavMeshConverterWorld>.Create(WorldConfig.Default());
            World<TestNavMeshConverterWorld>.Types().Component<NavMeshAgentComponent>();
            World<TestNavMeshConverterWorld>.Initialize();
            _host = new GameObject("NavMesh converter host");
            _host.AddComponent<NavMeshAgent>();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_host);
            World<TestNavMeshConverterWorld>.Destroy();
        }

        [Test]
        public void SerializableAndMonoVariantsResolveSameHostAgent()
        {
            var monoEntity = World<TestNavMeshConverterWorld>.NewEntity<Default>();
            var serializableEntity = World<TestNavMeshConverterWorld>.NewEntity<Default>();
            var mono = _host.AddComponent<TestNavMeshMovementConverter>();
            var serializable = new NavMeshMovementSerializableConverter<TestNavMeshConverterWorld>();

            mono.Apply(monoEntity, _host);
            serializable.Apply(serializableEntity, _host);

            var expected = _host.GetComponent<NavMeshAgent>();
            Assert.That(monoEntity.Read<NavMeshAgentComponent>().Agent, Is.SameAs(expected));
            Assert.That(serializableEntity.Read<NavMeshAgentComponent>().Agent, Is.SameAs(expected));
        }
    }

    public sealed class TestNavMeshMovementConverter : NavMeshMovementConverter<TestNavMeshConverterWorld> { }

    public struct TestNavMeshConverterWorld : IWorldType { }
}
