using FFS.Libraries.StaticEcs;
using NUnit.Framework;
using unigame.staticecs.Time;

namespace unigame.staticecs.features.Tests {
    [TestFixture]
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

        private static IAbilityStepConfig InstantTree() => new WaitStepConfig(0f);

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
        public void IsReady_ReturnsFalse_WhenNotEquipped() {
            var registry = World<TestAbilityWorld>.GetResource<AbilityRegistry<TestAbilityWorld>>();
            registry.Register(new AbilityDefinition(AbilityA), InstantTree());

            var caster = World<TestAbilityWorld>.NewEntity<Default>();
            Assert.IsFalse(AbilityOperations.IsReady<TestAbilityWorld>(caster.GID, AbilityA));
        }

        [Test]
        public void IsReady_ReturnsTrue_WhenEquippedAndRegistered() {
            var registry = World<TestAbilityWorld>.GetResource<AbilityRegistry<TestAbilityWorld>>();
            registry.Register(new AbilityDefinition(AbilityA), InstantTree());

            var caster = World<TestAbilityWorld>.NewEntity<Default>();
            AbilityOperations.Equip<TestAbilityWorld>(caster.GID, AbilityA);

            Assert.IsTrue(AbilityOperations.IsReady<TestAbilityWorld>(caster.GID, AbilityA));
        }

        [Test]
        public void IsReady_IgnoresCooldown_BusinessLayerResponsibility() {
            // §1b: ability slice does not gate on cooldown. CooldownOperations is consulted by
            // the caller before TryStartCast, but IsReady itself stays cooldown-agnostic.
            var registry = World<TestAbilityWorld>.GetResource<AbilityRegistry<TestAbilityWorld>>();
            registry.Register(new AbilityDefinition(AbilityA), InstantTree());

            var caster = World<TestAbilityWorld>.NewEntity<Default>();
            AbilityOperations.Equip<TestAbilityWorld>(caster.GID, AbilityA);
            CooldownOperations.Trigger<TestAbilityWorld>(caster.GID, AbilityA, 5f);

            Assert.IsTrue(AbilityOperations.IsReady<TestAbilityWorld>(caster.GID, AbilityA));
            Assert.IsFalse(CooldownOperations.IsReady<TestAbilityWorld>(caster.GID, AbilityA));
        }

        [Test]
        public void TryStartCast_FailsIfNotReady_AndQueuesEvent() {
            var registry = World<TestAbilityWorld>.GetResource<AbilityRegistry<TestAbilityWorld>>();
            registry.Register(new AbilityDefinition(AbilityA), InstantTree());

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
        public void IsCasting_TrueWhileActiveCastRefPresent() {
            var caster = World<TestAbilityWorld>.NewEntity<Default>();
            Assert.IsFalse(AbilityOperations.IsCasting<TestAbilityWorld>(caster.GID));

            caster.Set(new AbilityActiveCastRef { Cast = caster.GID });
            Assert.IsTrue(AbilityOperations.IsCasting<TestAbilityWorld>(caster.GID));
        }
    }
}
