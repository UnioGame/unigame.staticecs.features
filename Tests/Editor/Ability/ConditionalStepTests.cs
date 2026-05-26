using System.Collections.Generic;
using FFS.Libraries.StaticEcs;
using NUnit.Framework;
using unigame.staticecs.Time;
using unigame.staticecs.unity;
using UnityEngine;

namespace unigame.staticecs.features.Tests {
    [TestFixture]
    public sealed class ConditionalStepTests {
        private static readonly AbilityId Ability = new(302);

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
            _progressionSystem.Init();
        }

        [TearDown]
        public void TearDown() {
            _castSystem.Destroy();
            _progressionSystem.Destroy();
            foreach (var go in _objects) {
                Object.DestroyImmediate(go);
            }
            _objects.Clear();
            if (World<TestAbilityWorld>.Status != WorldStatus.NotCreated) {
                World<TestAbilityWorld>.Destroy();
            }
        }

        [Test]
        public void AoeNonEmpty_SelectsTrueBranch() {
            RegisterConditional();
            var caster = CreateEntity("caster", Vector3.zero, targetable: true);
            CreateEntity("target", Vector3.right, targetable: true);
            World<TestAbilityWorld>.GetResource<ITargetIndex<TestAbilityWorld>>().Rebuild();

            var amount = RunAndReadDamageAmount(caster);

            Assert.AreEqual(1f, amount);
        }

        [Test]
        public void AoeEmpty_SelectsFalseBranch() {
            RegisterConditional();
            var caster = CreateEntity("caster", Vector3.zero, targetable: true);
            World<TestAbilityWorld>.GetResource<ITargetIndex<TestAbilityWorld>>().Rebuild();

            var amount = RunAndReadDamageAmount(caster);

            Assert.AreEqual(2f, amount);
        }

        private static void RegisterConditional() {
            var registry = World<TestAbilityWorld>.GetResource<AbilityRegistry<TestAbilityWorld>>();
            registry.Register(new AbilityDefinition(Ability), new SequenceStepConfig(new IAbilityStepConfig[] {
                new AoeQueryStepConfig(radius: 3f, maxTargets: 8, excludeCaster: true),
                new ConditionalStepConfig(
                    new AoeNonEmptyCondition(),
                    new ApplyDamageStepConfig(1f, AbilityTargetMode.Self),
                    new ApplyDamageStepConfig(2f, AbilityTargetMode.Self)),
            }));
        }

        private float RunAndReadDamageAmount(World<TestAbilityWorld>.Entity caster) {
            AbilityOperations.Equip<TestAbilityWorld>(caster.GID, Ability);
            var receiver = World<TestAbilityWorld>.RegisterEventReceiver<IncomingDamageEvent>();
            try {
                Assert.IsTrue(AbilityOperations.TryStartCast<TestAbilityWorld>(caster.GID, Ability));
                Tick(0.0001f);
                RunSystems();

                float? amount = null;
                foreach (var e in receiver) {
                    amount = e.Value.Amount;
                }

                Assert.IsTrue(amount.HasValue);
                return amount.Value;
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
