using System.Collections.Generic;
using FFS.Libraries.StaticEcs;
using NUnit.Framework;
using unigame.staticecs.Time;
using unigame.staticecs.unity;
using UnityEngine;

namespace unigame.staticecs.features.Tests {
    [TestFixture]
    public sealed class ApplyEffectStepTests {
        private static readonly AbilityId Ability = new(203);

        private readonly List<GameObject> _objects = new();
        private readonly List<EntityGID> _dispatchTargets = new();
        private AbilityCastSystem<TestAbilityWorld> _castSystem;
        private AbilityWaitSystem<TestAbilityWorld> _waitSystem;
        private AbilityStepProgressionSystem<TestAbilityWorld> _progressionSystem;

        [SetUp]
        public void SetUp() {
            World<TestAbilityWorld>.Create(WorldConfig.Default());
            new EcsTimeFeature<TestAbilityWorld>(registerFixed: false).RegisterTypes(World<TestAbilityWorld>.Types());
            new TargetSelectionFeature<TestAbilityWorld>(registerRebuildSystem: false).RegisterTypes(World<TestAbilityWorld>.Types());
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
        public void ApplyEffect_DispatchesByEffectId_ForAoeBroadcast() {
            var effectId = RegisterEffectDispatcher();
            var registry = World<TestAbilityWorld>.GetResource<AbilityRegistry<TestAbilityWorld>>();
            registry.Register(new AbilityDefinition(Ability), new SequenceStepConfig(new IAbilityStepConfig[] {
                new AoeQueryStepConfig(radius: 3f, maxTargets: 8, excludeCaster: true),
                new ApplyEffectStepConfig(effectId, AbilityTargetMode.AoeBroadcast, duration: 2f),
            }));

            var caster = CreateEntity("caster", Vector3.zero, targetable: true);
            var nearA = CreateEntity("near-a", new Vector3(1f, 0f, 0f), targetable: true);
            var nearB = CreateEntity("near-b", new Vector3(2f, 0f, 0f), targetable: true);
            CreateEntity("far", new Vector3(9f, 0f, 0f), targetable: true);

            World<TestAbilityWorld>.GetResource<ITargetIndex<TestAbilityWorld>>().Rebuild();
            AbilityOperations.Equip<TestAbilityWorld>(caster.GID, Ability);
            AbilityOperations.TryStartCast<TestAbilityWorld>(caster.GID, Ability);

            Tick(0.0001f);
            RunSystems();

            Assert.AreEqual(2, _dispatchTargets.Count);
            CollectionAssert.Contains(_dispatchTargets, nearA.GID);
            CollectionAssert.Contains(_dispatchTargets, nearB.GID);
            CollectionAssert.DoesNotContain(_dispatchTargets, caster.GID);
        }

        private EffectId RegisterEffectDispatcher() {
            var ids = new EffectIdRegistry();
            var effectId = ids.Register<TestAbilityEffect>();
            World<TestAbilityWorld>.SetResource(ids);

            var dispatch = World<TestAbilityWorld>.GetResource<AbilityEffectDispatchRegistry<TestAbilityWorld>>();
            dispatch.Register(effectId, (source, target, duration, period, delay, magnitude) => {
                _dispatchTargets.Add(target);
                return duration > 0f;
            });
            return effectId;
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

        [EffectFlag(EffectFlag.Reserved0)]
        private struct TestAbilityEffect : IEffectType { }
    }
}
