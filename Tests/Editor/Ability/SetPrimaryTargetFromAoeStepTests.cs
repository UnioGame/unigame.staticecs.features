using System.Collections.Generic;
using FFS.Libraries.StaticEcs;
using NUnit.Framework;
using unigame.staticecs.Time;
using unigame.staticecs.unity;
using UnityEngine;

namespace unigame.staticecs.features.Tests {
    [TestFixture]
    public sealed class SetPrimaryTargetFromAoeStepTests {
        private static readonly AbilityId Ability = new(202);

        private readonly List<GameObject> _objects = new();
        private AbilityCastSystem<TestAbilityWorld> _castSystem;
        private AbilityWaitSystem<TestAbilityWorld> _waitSystem;
        private AbilityStepProgressionSystem<TestAbilityWorld> _progressionSystem;
        private FakeAbilityRng _rng;

        [SetUp]
        public void SetUp() {
            World<TestAbilityWorld>.Create(WorldConfig.Default());
            new EcsTimeFeature<TestAbilityWorld>(registerFixed: false).RegisterTypes(World<TestAbilityWorld>.Types());
            new TargetSelectionFeature<TestAbilityWorld>(registerRebuildSystem: false).RegisterTypes(World<TestAbilityWorld>.Types());
            new DamageFeature<TestAbilityWorld>(registerApplySystem: false).RegisterTypes(World<TestAbilityWorld>.Types());
            new AbilityFeature<TestAbilityWorld>(registerSystems: false).RegisterTypes(World<TestAbilityWorld>.Types());
            World<TestAbilityWorld>.Types().Component<TransformBindingComponent>();
            World<TestAbilityWorld>.Initialize();

            _rng = new FakeAbilityRng();
            World<TestAbilityWorld>.SetResource<IAbilityRng<TestAbilityWorld>>(_rng);

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
        public void First_SelectsFirstAoeEntry() {
            AssertSelector(AoeTargetSelector.First, randomIndex: 0, expectedX: 1f);
        }

        [Test]
        public void Random_UsesAbilityRngIndex() {
            AssertSelector(AoeTargetSelector.Random, randomIndex: 1, expectedX: 2f);
        }

        [Test]
        public void Closest_SelectsNearestToCaster() {
            AssertSelector(AoeTargetSelector.Closest, randomIndex: 0, expectedX: 1f);
        }

        [Test]
        public void EmptyBuffer_FailsCast() {
            RegisterSelectorAbility(AoeTargetSelector.First);
            var caster = CreateEntity("caster", Vector3.zero, targetable: false);
            World<TestAbilityWorld>.GetResource<ITargetIndex<TestAbilityWorld>>().Rebuild();
            AbilityOperations.Equip<TestAbilityWorld>(caster.GID, Ability);

            var receiver = World<TestAbilityWorld>.RegisterEventReceiver<AbilityCompletedEvent>();
            try {
                AbilityOperations.TryStartCast<TestAbilityWorld>(caster.GID, Ability);
                Tick(0.0001f);
                RunSystems();

                AbilityCompletedReason? reason = null;
                foreach (var e in receiver) {
                    reason = e.Value.Reason;
                }
                Assert.AreEqual(AbilityCompletedReason.Cancelled, reason);
            } finally {
                World<TestAbilityWorld>.DeleteEventReceiver(ref receiver);
            }
        }

        private void AssertSelector(AoeTargetSelector selector, int randomIndex, float expectedX) {
            RegisterSelectorAbility(selector);
            _rng.NextInt = randomIndex;

            var caster = CreateEntity("caster", Vector3.zero, targetable: false);
            var first = CreateEntity("first", new Vector3(1f, 0f, 0f), targetable: true);
            var second = CreateEntity("second", new Vector3(2f, 0f, 0f), targetable: true);
            var expected = Mathf.Approximately(expectedX, 1f) ? first.GID : second.GID;

            World<TestAbilityWorld>.GetResource<ITargetIndex<TestAbilityWorld>>().Rebuild();
            AbilityOperations.Equip<TestAbilityWorld>(caster.GID, Ability);

            var receiver = World<TestAbilityWorld>.RegisterEventReceiver<IncomingDamageEvent>();
            try {
                AbilityOperations.TryStartCast<TestAbilityWorld>(caster.GID, Ability);
                Tick(0.0001f);
                RunSystems();

                EntityGID actual = default;
                foreach (var e in receiver) {
                    actual = e.Value.Target;
                }
                Assert.AreEqual(expected, actual);
            } finally {
                World<TestAbilityWorld>.DeleteEventReceiver(ref receiver);
            }
        }

        private static void RegisterSelectorAbility(AoeTargetSelector selector) {
            var registry = World<TestAbilityWorld>.GetResource<AbilityRegistry<TestAbilityWorld>>();
            registry.Register(new AbilityDefinition(Ability), new SequenceStepConfig(new IAbilityStepConfig[] {
                new AoeQueryStepConfig(radius: 3f, maxTargets: 8, excludeCaster: true),
                new SetPrimaryTargetFromAoeStepConfig(selector),
                new ApplyDamageStepConfig(10f, AbilityTargetMode.PrimaryTarget),
            }));
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
