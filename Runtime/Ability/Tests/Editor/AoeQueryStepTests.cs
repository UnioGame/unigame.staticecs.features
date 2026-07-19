using System.Collections.Generic;
using FFS.Libraries.StaticEcs;
using NUnit.Framework;
using UniGame.StaticEcs.Time;
using UniGame.StaticEcs.Unity;
 
 
using UnityEngine;

namespace UniGame.StaticEcs.Features.Tests {
    [TestFixture]
    public sealed class AoeQueryStepTests {
        private static readonly AbilityId Ability = new(201);

        private readonly List<GameObject> _objects = new();
        private AbilityCastSystem<TestAbilityWorld> _castSystem;
        private AbilityWaitSystem<TestAbilityWorld> _waitSystem;
        private AbilityStepProgressionSystem<TestAbilityWorld> _progressionSystem;

        [SetUp]
        public void SetUp() {
            World<TestAbilityWorld>.Create(WorldConfig.Default());
            new EcsTimeFeature<TestAbilityWorld>(registerFixed: false).RegisterTypes(World<TestAbilityWorld>.Types());
            new TargetSelectionFeature<TestAbilityWorld>(registerRebuildSystem: false).RegisterTypes(World<TestAbilityWorld>.Types());
            new DamageFeature<TestAbilityWorld>(registerApplySystem: false).RegisterTypes(World<TestAbilityWorld>.Types());
            new AbilityFeature<TestAbilityWorld>(registerSystems: false).RegisterTypes(World<TestAbilityWorld>.Types());
            World<TestAbilityWorld>.Types().Component<TransformBindingComponent>();
            World<TestAbilityWorld>.Initialize();

            _castSystem = new AbilityCastSystem<TestAbilityWorld>();
            _waitSystem = new AbilityWaitSystem<TestAbilityWorld>();
            _progressionSystem = new AbilityStepProgressionSystem<TestAbilityWorld>();
            _castSystem.Init();
        }

        [TearDown]
        public void TearDown() {
            _castSystem.Destroy();
            foreach (var go in _objects) {
                Object.DestroyImmediate(go);
            }
            _objects.Clear();
            if (World<TestAbilityWorld>.Status != WorldStatus.NotCreated) {
                World<TestAbilityWorld>.Destroy();
            }
        }

        [Test]
        public void AoeQuery_FillsBuffer_AndApplyDamageBroadcastsToTargets() {
            var registry = World<TestAbilityWorld>.GetResource<AbilityRegistry<TestAbilityWorld>>();
            registry.Register(new AbilityDefinition(Ability), new SequenceStepConfig(new IAbilityStepConfig[] {
                new AoeQueryStepConfig(radius: 3f, maxTargets: 8, excludeCaster: true),
                new ApplyDamageStepConfig(10f, AbilityTargetMode.AoeBroadcast),
            }));

            var caster = CreateEntity("caster", Vector3.zero, targetable: true);
            var nearA = CreateEntity("near-a", new Vector3(1f, 0f, 0f), targetable: true);
            var nearB = CreateEntity("near-b", new Vector3(0f, 0f, 2f), targetable: true);
            CreateEntity("far", new Vector3(10f, 0f, 0f), targetable: true);

            World<TestAbilityWorld>.GetResource<ITargetIndex<TestAbilityWorld>>().Rebuild();
            AbilityOperations.Equip<TestAbilityWorld>(caster.GID, Ability);

            var receiver = World<TestAbilityWorld>.RegisterEventReceiver<IncomingDamageEvent>();
            try {
                Assert.IsTrue(AbilityOperations.TryStartCast<TestAbilityWorld>(caster.GID, Ability));
                Tick(0.0001f);
                RunSystems();

                var hits = 0;
                var hitA = false;
                var hitB = false;
                foreach (var e in receiver) {
                    hits++;
                    if (e.Value.Target.Equals(nearA.GID)) hitA = true;
                    if (e.Value.Target.Equals(nearB.GID)) hitB = true;
                    Assert.AreNotEqual(caster.GID, e.Value.Target);
                }

                Assert.AreEqual(2, hits);
                Assert.IsTrue(hitA);
                Assert.IsTrue(hitB);
            } finally {
                World<TestAbilityWorld>.DeleteEventReceiver(ref receiver);
            }
        }

        private World<TestAbilityWorld>.Entity CreateEntity(string name, Vector3 position, bool targetable) {
            var go = new GameObject(name);
            _objects.Add(go);
            go.transform.position = position;

            var entity = World<TestAbilityWorld>.NewEntity<Default>();
            entity.Set(new TransformBindingComponent { Transform = go.transform });
            if (targetable) {
                entity.Set<TargetableTag>();
            }
            return entity;
        }

        private static void Tick(float dt) {
            ref var time = ref World<TestAbilityWorld>.GetResource<EcsTime>();
            time.DeltaTime = dt;
            time.Now += dt;
        }

        private void RunSystems() {
            _castSystem.Update();
            _waitSystem.Update();
            _progressionSystem.Update();
        }
    }
}
