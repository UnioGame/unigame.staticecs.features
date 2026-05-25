using FFS.Libraries.StaticEcs;
using NUnit.Framework;
using unigame.staticecs.Time;

namespace unigame.staticecs.features.Tests {
    public sealed class AbilityTickSystemTests {
        private static readonly AbilityId Ability = new(7);
        private RecordingAbilityHandler<TestAbilityWorld> _handler;
        private AbilityCastSystem<TestAbilityWorld> _castSystem;
        private AbilityTickSystem<TestAbilityWorld> _tickSystem;

        [SetUp]
        public void SetUp() {
            World<TestAbilityWorld>.Create(WorldConfig.Default());
            new EcsTimeFeature<TestAbilityWorld>(registerFixed: false).RegisterTypes(World<TestAbilityWorld>.Types());
            new AbilityFeature<TestAbilityWorld>(registerSystems: false).RegisterTypes(World<TestAbilityWorld>.Types());
            World<TestAbilityWorld>.Initialize();

            _handler = new RecordingAbilityHandler<TestAbilityWorld>();
            _castSystem = new AbilityCastSystem<TestAbilityWorld>();
            _tickSystem = new AbilityTickSystem<TestAbilityWorld>();
            _castSystem.Init();
        }

        [TearDown]
        public void TearDown() {
            _castSystem.Destroy();
            if (World<TestAbilityWorld>.Status != WorldStatus.NotCreated) {
                World<TestAbilityWorld>.Destroy();
            }
        }

        private static void Tick(float dt) {
            ref var time = ref World<TestAbilityWorld>.GetResource<EcsTime>();
            time.DeltaTime = dt;
            time.Now += dt;
        }

        [Test]
        public void Cast_WithChargingAndCasting_FiresHandlerOnce_OnLeavingCharging() {
            var registry = World<TestAbilityWorld>.GetResource<AbilityRegistry<TestAbilityWorld>>();
            registry.Register(new AbilityDefinition(Ability, 0.5f, 0.5f, 0.5f, 2f), _handler);

            var caster = World<TestAbilityWorld>.NewEntity<Default>();
            AbilityOperations.Equip<TestAbilityWorld>(caster.GID, Ability);
            Assert.IsTrue(AbilityOperations.TryStartCast<TestAbilityWorld>(caster.GID, Ability));

            Tick(0.0001f);
            _castSystem.Update();
            Assert.AreEqual(0, _handler.Invocations.Count);
            Assert.AreEqual(AbilityPhase.Charging, caster.Read<AbilityCastComponent>().Phase);

            Tick(0.6f);
            _tickSystem.Update();
            Assert.AreEqual(1, _handler.Invocations.Count);
            Assert.AreEqual(AbilityPhase.Casting, caster.Read<AbilityCastComponent>().Phase);

            Tick(0.6f);
            _tickSystem.Update();
            Assert.AreEqual(AbilityPhase.Recovering, caster.Read<AbilityCastComponent>().Phase);

            Tick(0.6f);
            _tickSystem.Update();
            Assert.IsFalse(caster.Has<AbilityCastComponent>());
            Assert.IsFalse(CooldownOperations.IsReady<TestAbilityWorld>(caster.GID, Ability));
        }

        [Test]
        public void InstantCast_ZeroDurations_FiresHandlerImmediately_AndCompletes() {
            var registry = World<TestAbilityWorld>.GetResource<AbilityRegistry<TestAbilityWorld>>();
            registry.Register(new AbilityDefinition(Ability, 0f, 0f, 0f, 1f), _handler);

            var caster = World<TestAbilityWorld>.NewEntity<Default>();
            AbilityOperations.Equip<TestAbilityWorld>(caster.GID, Ability);
            Assert.IsTrue(AbilityOperations.TryStartCast<TestAbilityWorld>(caster.GID, Ability));

            Tick(0.0001f);
            _castSystem.Update();

            Assert.AreEqual(1, _handler.Invocations.Count);
            Assert.IsFalse(caster.Has<AbilityCastComponent>());
            Assert.IsFalse(CooldownOperations.IsReady<TestAbilityWorld>(caster.GID, Ability));
        }

        [Test]
        public void Cast_EmitsStartedAndCompletedStateEvents() {
            var registry = World<TestAbilityWorld>.GetResource<AbilityRegistry<TestAbilityWorld>>();
            registry.Register(new AbilityDefinition(Ability, 0.2f, 0f, 0f, 0.5f), _handler);

            var caster = World<TestAbilityWorld>.NewEntity<Default>();
            AbilityOperations.Equip<TestAbilityWorld>(caster.GID, Ability);
            AbilityOperations.TryStartCast<TestAbilityWorld>(caster.GID, Ability);

            var receiver = World<TestAbilityWorld>.RegisterEventReceiver<AbilityStateChangedEvent>();
            try {
                Tick(0.0001f);
                _castSystem.Update();

                Tick(0.3f);
                _tickSystem.Update();

                var started = 0;
                var completed = 0;
                foreach (var e in receiver) {
                    if (e.Value.Reason == AbilityChangeReason.Started) started++;
                    if (e.Value.Reason == AbilityChangeReason.Completed) completed++;
                }
                Assert.AreEqual(1, started);
                Assert.AreEqual(1, completed);
            } finally {
                World<TestAbilityWorld>.DeleteEventReceiver(ref receiver);
            }
        }
    }
}
