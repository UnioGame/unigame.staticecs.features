using FFS.Libraries.StaticEcs;
using NUnit.Framework;
using unigame.staticecs.Time;

namespace unigame.staticecs.features.Tests {
    public sealed class AbilityOperationsTests {
        private static readonly AbilityId AbilityA = new(1);
        private static readonly AbilityId AbilityB = new(2);

        [SetUp]
        public void SetUp() {
            World<TestAbilityWorld>.Create(WorldConfig.Default());
            new EcsTimeFeature<TestAbilityWorld>(registerFixed: false).RegisterTypes(World<TestAbilityWorld>.Types());
            new AbilityFeature<TestAbilityWorld>(registerSystems: false).RegisterTypes(World<TestAbilityWorld>.Types());
            World<TestAbilityWorld>.Initialize();
        }

        [TearDown]
        public void TearDown() {
            if (World<TestAbilityWorld>.Status != WorldStatus.NotCreated) {
                World<TestAbilityWorld>.Destroy();
            }
        }

        [Test]
        public void Equip_AddsRosterEntry_DuplicateIgnored() {
            var caster = World<TestAbilityWorld>.NewEntity<Default>();

            Assert.IsTrue(AbilityOperations.Equip<TestAbilityWorld>(caster.GID, AbilityA));
            Assert.IsFalse(AbilityOperations.Equip<TestAbilityWorld>(caster.GID, AbilityA));
            Assert.IsTrue(AbilityOperations.Equip<TestAbilityWorld>(caster.GID, AbilityB));
            Assert.IsTrue(AbilityOperations.HasAbility<TestAbilityWorld>(caster.GID, AbilityA));
            Assert.IsTrue(AbilityOperations.HasAbility<TestAbilityWorld>(caster.GID, AbilityB));
        }

        [Test]
        public void Unequip_RemovesEntry_AndIsIdempotent() {
            var caster = World<TestAbilityWorld>.NewEntity<Default>();
            AbilityOperations.Equip<TestAbilityWorld>(caster.GID, AbilityA);

            Assert.IsTrue(AbilityOperations.Unequip<TestAbilityWorld>(caster.GID, AbilityA));
            Assert.IsFalse(AbilityOperations.Unequip<TestAbilityWorld>(caster.GID, AbilityA));
            Assert.IsFalse(AbilityOperations.HasAbility<TestAbilityWorld>(caster.GID, AbilityA));
        }

        [Test]
        public void IsReady_ReturnsFalse_WhenAbilityNotRegistered() {
            var caster = World<TestAbilityWorld>.NewEntity<Default>();
            AbilityOperations.Equip<TestAbilityWorld>(caster.GID, AbilityA);

            Assert.IsFalse(AbilityOperations.IsReady<TestAbilityWorld>(caster.GID, AbilityA));
        }

        [Test]
        public void IsReady_ReturnsTrue_WhenEquippedRegisteredAndOffCooldown() {
            var registry = World<TestAbilityWorld>.GetResource<AbilityRegistry<TestAbilityWorld>>();
            registry.Register(new AbilityDefinition(AbilityA, 0f, 0f, 0f, 1f), (_, _) => { });

            var caster = World<TestAbilityWorld>.NewEntity<Default>();
            AbilityOperations.Equip<TestAbilityWorld>(caster.GID, AbilityA);

            Assert.IsTrue(AbilityOperations.IsReady<TestAbilityWorld>(caster.GID, AbilityA));
        }

        [Test]
        public void TryStartCast_FailsIfNotReady_AndQueuesEvent() {
            var registry = World<TestAbilityWorld>.GetResource<AbilityRegistry<TestAbilityWorld>>();
            registry.Register(new AbilityDefinition(AbilityA, 0f, 0f, 0f, 1f), (_, _) => { });

            var caster = World<TestAbilityWorld>.NewEntity<Default>();
            Assert.IsFalse(AbilityOperations.TryStartCast<TestAbilityWorld>(caster.GID, AbilityA));

            AbilityOperations.Equip<TestAbilityWorld>(caster.GID, AbilityA);
            var receiver = World<TestAbilityWorld>.RegisterEventReceiver<CastAbilityEvent>();
            try {
                Assert.IsTrue(AbilityOperations.TryStartCast<TestAbilityWorld>(caster.GID, AbilityA));
                var queued = 0;
                foreach (var _ in receiver) {
                    queued++;
                }
                Assert.AreEqual(1, queued);
            } finally {
                World<TestAbilityWorld>.DeleteEventReceiver(ref receiver);
            }
        }

        [Test]
        public void Cancel_DeletesActiveCastAndEmitsEvent() {
            var caster = World<TestAbilityWorld>.NewEntity<Default>();
            caster.Set(new AbilityCastComponent {
                AbilityId = AbilityA,
                Phase = AbilityPhase.Charging,
                TimeLeft = 1f,
            });

            var receiver = World<TestAbilityWorld>.RegisterEventReceiver<AbilityStateChangedEvent>();
            try {
                Assert.IsTrue(AbilityOperations.Cancel<TestAbilityWorld>(caster.GID));
                Assert.IsFalse(caster.Has<AbilityCastComponent>());

                var cancelledEvents = 0;
                foreach (var e in receiver) {
                    if (e.Value.Reason == AbilityChangeReason.Cancelled) {
                        cancelledEvents++;
                    }
                }
                Assert.AreEqual(1, cancelledEvents);
            } finally {
                World<TestAbilityWorld>.DeleteEventReceiver(ref receiver);
            }
        }
    }
}
