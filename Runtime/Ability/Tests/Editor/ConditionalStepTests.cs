namespace UniGame.StaticEcs.Features.Tests
{
    using System.Collections.Generic;
    using FFS.Libraries.StaticEcs;
    using NUnit.Framework;
    using UniGame.StaticEcs.Tests;
    using UniGame.StaticEcs.Time;
    using UniGame.StaticEcs.Unity;
    using UnityEngine;

    [TestFixture]
    public sealed class ConditionalStepTests
    {
        private static readonly AbilityId Ability = new(302);

        private readonly List<GameObject> _objects = new();
        private AbilityCastSystem<TestAbilityWorld> _castSystem;
        private AbilityWaitSystem<TestAbilityWorld> _waitSystem;
        private AbilityStepProgressionSystem<TestAbilityWorld> _progressionSystem;
        private StaticEcsTestWorld<TestAbilityWorld> _world;

        [SetUp]
        public void SetUp()
        {
            _world = new StaticEcsTestWorld<TestAbilityWorld>();
            new EcsTimeFeature<TestAbilityWorld>().InstallResourcesAndRegisterTypesForTest(
                _world
            );
            new TargetSelectionFeature<TestAbilityWorld>().InstallResourcesAndRegisterTypesForTest(
                _world);
            new DamageFeature<TestAbilityWorld>().InstallResourcesAndRegisterTypesForTest(
                _world
            );
            new AbilityFeature<TestAbilityWorld>().InstallResourcesAndRegisterTypesForTest(
                _world
            );
            _world.Types.Component<TransformComponent>();
            _world.Initialize();

            _castSystem = new AbilityCastSystem<TestAbilityWorld>();
            _waitSystem = new AbilityWaitSystem<TestAbilityWorld>();
            _progressionSystem = new AbilityStepProgressionSystem<TestAbilityWorld>();
            _castSystem.Init();
            _progressionSystem.Init();
        }

        [TearDown]
        public void TearDown()
        {
            _world?.TerminateLifeTime();
            _castSystem.Destroy();
            _progressionSystem.Destroy();
            foreach (var go in _objects)
            {
                Object.DestroyImmediate(go);
            }
            _objects.Clear();
            _world?.Dispose();
        }

        [Test]
        public void AoeNonEmpty_SelectsTrueBranch()
        {
            RegisterConditional();
            var caster = CreateEntity("caster", Vector3.zero, targetable: true);
            CreateEntity("target", Vector3.right, targetable: true);
            World<TestAbilityWorld>.GetResource<ITargetIndex<TestAbilityWorld>>().Rebuild();

            var amount = RunAndReadDamageAmount(caster);

            Assert.AreEqual(1f, amount);
        }

        [Test]
        public void AoeEmpty_SelectsFalseBranch()
        {
            RegisterConditional();
            var caster = CreateEntity("caster", Vector3.zero, targetable: true);
            World<TestAbilityWorld>.GetResource<ITargetIndex<TestAbilityWorld>>().Rebuild();

            var amount = RunAndReadDamageAmount(caster);

            Assert.AreEqual(2f, amount);
        }

        private static void RegisterConditional()
        {
            var registry = World<TestAbilityWorld>.GetResource<AbilityRegistry<TestAbilityWorld>>();
            registry.Register(
                new AbilityDefinition(Ability),
                new SequenceStepConfig(
                    new IAbilityStepConfig[]
                    {
                        new AoeQueryStepConfig(radius: 3f, maxTargets: 8, excludeCaster: true),
                        new ConditionalStepConfig(
                            new AoeNonEmptyCondition(),
                            new ApplyDamageStepConfig(1f, AbilityTargetMode.Self),
                            new ApplyDamageStepConfig(2f, AbilityTargetMode.Self)
                        ),
                    }
                )
            );
        }

        private float RunAndReadDamageAmount(World<TestAbilityWorld>.Entity caster)
        {
            AbilityOperations.Equip<TestAbilityWorld>(caster.GID, Ability);
            var receiver = World<TestAbilityWorld>.RegisterEventReceiver<IncomingDamageEvent>();
            try
            {
                Assert.IsTrue(
                    AbilityOperations.TryStartCast<TestAbilityWorld>(caster.GID, Ability)
                );
                Tick(0.0001f);
                RunSystems();

                float? amount = null;
                foreach (var e in receiver)
                {
                    amount = e.Value.Amount;
                }

                Assert.IsTrue(amount.HasValue);
                return amount.Value;
            }
            finally
            {
                World<TestAbilityWorld>.DeleteEventReceiver(ref receiver);
            }
        }

        private World<TestAbilityWorld>.Entity CreateEntity(
            string name,
            Vector3 position,
            bool targetable
        )
        {
            var go = new GameObject(name);
            _objects.Add(go);
            go.transform.position = position;

            var entity = World<TestAbilityWorld>.NewEntity<Default>();
            entity.Set(new TransformComponent { Transform = go.transform });
            if (targetable)
                entity.Set<TargetableTag>();
            return entity;
        }

        private static void Tick(float dt)
        {
            ref var time = ref World<TestAbilityWorld>.GetResource<EcsTime>();
            time.DeltaTime = dt;
            time.Now += dt;
        }

        private void RunSystems()
        {
            _castSystem.Update();
            _waitSystem.Update();
            _progressionSystem.Update();
        }
    }
}
