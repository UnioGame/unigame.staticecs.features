using FFS.Libraries.StaticEcs;
using NUnit.Framework;
using unigame.staticecs.Time;

namespace unigame.staticecs.features.Tests {
    public sealed class CooldownOperationsTests {
        private static readonly AbilityId Ability = new(42);

        [SetUp]
        public void SetUp() {
            World<TestAbilityWorld>.Create(WorldConfig.Default());
            new EcsTimeFeature<TestAbilityWorld>(registerFixed: false).RegisterTypes(World<TestAbilityWorld>.Types());
            new AbilityFeature<TestAbilityWorld>(registerSystems: false).RegisterTypes(World<TestAbilityWorld>.Types());
            World<TestAbilityWorld>.Initialize();
            SetNow(0f);
        }

        [TearDown]
        public void TearDown() {
            if (World<TestAbilityWorld>.Status != WorldStatus.NotCreated) {
                World<TestAbilityWorld>.Destroy();
            }
        }

        private static void SetNow(float value) {
            ref var time = ref World<TestAbilityWorld>.GetResource<EcsTime>();
            time.Now = value;
        }

        [Test]
        public void IsReady_TrueWhenNoEntry() {
            var caster = World<TestAbilityWorld>.NewEntity<Default>();
            Assert.IsTrue(CooldownOperations.IsReady<TestAbilityWorld>(caster.GID, Ability));
        }

        [Test]
        public void Trigger_BlocksUntilExpiry_AndIsReadyAfter() {
            var caster = World<TestAbilityWorld>.NewEntity<Default>();
            CooldownOperations.Trigger<TestAbilityWorld>(caster.GID, Ability, 2f);

            Assert.IsFalse(CooldownOperations.IsReady<TestAbilityWorld>(caster.GID, Ability));
            Assert.AreEqual(2f, CooldownOperations.Remaining<TestAbilityWorld>(caster.GID, Ability), 1e-4);

            SetNow(1f);
            Assert.AreEqual(1f, CooldownOperations.Remaining<TestAbilityWorld>(caster.GID, Ability), 1e-4);

            SetNow(2.5f);
            Assert.IsTrue(CooldownOperations.IsReady<TestAbilityWorld>(caster.GID, Ability));
            Assert.AreEqual(0f, CooldownOperations.Remaining<TestAbilityWorld>(caster.GID, Ability), 1e-4);
        }

        [Test]
        public void IsReady_EmitsCooldownReadyEvent_OnFirstTimeAfterExpiry() {
            var caster = World<TestAbilityWorld>.NewEntity<Default>();
            CooldownOperations.Trigger<TestAbilityWorld>(caster.GID, Ability, 1f);
            SetNow(1.5f);

            var receiver = World<TestAbilityWorld>.RegisterEventReceiver<CooldownReadyEvent>();
            try {
                Assert.IsTrue(CooldownOperations.IsReady<TestAbilityWorld>(caster.GID, Ability));
                var observed = 0;
                foreach (var _ in receiver) {
                    observed++;
                }
                Assert.AreEqual(1, observed);
            } finally {
                World<TestAbilityWorld>.DeleteEventReceiver(ref receiver);
            }
        }

        [Test]
        public void Reset_RemovesEntryImmediately() {
            var caster = World<TestAbilityWorld>.NewEntity<Default>();
            CooldownOperations.Trigger<TestAbilityWorld>(caster.GID, Ability, 5f);
            Assert.IsFalse(CooldownOperations.IsReady<TestAbilityWorld>(caster.GID, Ability));

            Assert.IsTrue(CooldownOperations.Reset<TestAbilityWorld>(caster.GID, Ability));
            Assert.IsTrue(CooldownOperations.IsReady<TestAbilityWorld>(caster.GID, Ability));
        }

        [Test]
        public void Trigger_RefreshesExistingEntry() {
            var caster = World<TestAbilityWorld>.NewEntity<Default>();
            CooldownOperations.Trigger<TestAbilityWorld>(caster.GID, Ability, 1f);
            SetNow(0.5f);
            CooldownOperations.Trigger<TestAbilityWorld>(caster.GID, Ability, 3f);

            Assert.AreEqual(3f, CooldownOperations.Remaining<TestAbilityWorld>(caster.GID, Ability), 1e-4);
        }
    }
}
