using FFS.Libraries.StaticEcs;
using NUnit.Framework;
using UniGame.StaticEcs.Time;
 

namespace UniGame.StaticEcs.Features.Tests {
    [TestFixture]
    public sealed class HealOverTimeTests {
        private EffectTickSystem<TestEffectsWorld, HealOverTimeEffect> _tick;
        private ApplyDamageSystem<TestEffectsWorld> _apply;
        private FakeDamageRng _rng;

        [SetUp]
        public void SetUp() {
            World<TestEffectsWorld>.Create(WorldConfig.Default());
            _rng = new FakeDamageRng();
            World<TestEffectsWorld>.SetResource<IDamageRng>(_rng);

            new EcsTimeFeature<TestEffectsWorld>(registerFixed: false).RegisterTypes(World<TestEffectsWorld>.Types());
            new ModifierBackRefFeature<TestEffectsWorld>().RegisterTypes(World<TestEffectsWorld>.Types());
            new CharacteristicFeature<TestEffectsWorld, HealthCharacteristic>().RegisterTypes(World<TestEffectsWorld>.Types());
            new CharacteristicFeature<TestEffectsWorld, ShieldCharacteristic>().RegisterTypes(World<TestEffectsWorld>.Types());
            new CharacteristicFeature<TestEffectsWorld, BlockChanceCharacteristic>().RegisterTypes(World<TestEffectsWorld>.Types());
            new CharacteristicFeature<TestEffectsWorld, DodgeChanceCharacteristic>().RegisterTypes(World<TestEffectsWorld>.Types());
            new CharacteristicFeature<TestEffectsWorld, ArmorResistCharacteristic>().RegisterTypes(World<TestEffectsWorld>.Types());
            new CharacteristicFeature<TestEffectsWorld, CriticalChanceCharacteristic>().RegisterTypes(World<TestEffectsWorld>.Types());
            new CharacteristicFeature<TestEffectsWorld, CriticalMultiplierCharacteristic>().RegisterTypes(World<TestEffectsWorld>.Types());
            new DamageFeature<TestEffectsWorld>(registerApplySystem: false).RegisterTypes(World<TestEffectsWorld>.Types());
            new HealOverTimeFeature<TestEffectsWorld>(registerTickSystem: false).RegisterTypes(World<TestEffectsWorld>.Types());

            World<TestEffectsWorld>.Initialize();

            _tick = new EffectTickSystem<TestEffectsWorld, HealOverTimeEffect>();
            _apply = new ApplyDamageSystem<TestEffectsWorld>();
            _apply.Init();
        }

        [TearDown]
        public void TearDown() {
            if (World<TestEffectsWorld>.Status != WorldStatus.NotCreated) {
                _apply.Destroy();
                World<TestEffectsWorld>.Destroy();
            }
        }

        private static void Tick(float dt) {
            ref var time = ref World<TestEffectsWorld>.GetResource<EcsTime>();
            time.DeltaTime = dt;
        }

        [Test]
        public void TickRaisesHealing_AppliedToHealth() {
            var source = World<TestEffectsWorld>.NewEntity<Default>();
            var target = World<TestEffectsWorld>.NewEntity<Default>();
            target.Set(CharacteristicComponent<HealthCharacteristic>.Create(50f, 0f, 100f));

            HealOverTimeOperations.Apply<TestEffectsWorld>(target.GID, source.GID, healPerTick: 5f, duration: 5f, period: 1f);

            Tick(1f);
            _tick.Update();
            _apply.Update();

            Assert.AreEqual(55f, target.Read<CharacteristicComponent<HealthCharacteristic>>().Value, 0.0001f);

            Tick(2f);
            _tick.Update();
            _apply.Update();

            Assert.AreEqual(65f, target.Read<CharacteristicComponent<HealthCharacteristic>>().Value, 0.0001f);
        }

        [Test]
        public void HealScalesByStacks() {
            var source = World<TestEffectsWorld>.NewEntity<Default>();
            var target = World<TestEffectsWorld>.NewEntity<Default>();
            target.Set(CharacteristicComponent<HealthCharacteristic>.Create(50f, 0f, 100f));

            HealOverTimeOperations.Apply<TestEffectsWorld>(target.GID, source.GID, healPerTick: 5f, duration: 5f, period: 1f);
            HealOverTimeOperations.Apply<TestEffectsWorld>(target.GID, source.GID, healPerTick: 5f, duration: 5f, period: 1f);

            Tick(1f);
            _tick.Update();
            _apply.Update();

            Assert.AreEqual(60f, target.Read<CharacteristicComponent<HealthCharacteristic>>().Value, 0.0001f);
        }

        [Test]
        public void Expiry_StopsTicks() {
            var source = World<TestEffectsWorld>.NewEntity<Default>();
            var target = World<TestEffectsWorld>.NewEntity<Default>();
            target.Set(CharacteristicComponent<HealthCharacteristic>.Create(50f, 0f, 100f));

            HealOverTimeOperations.Apply<TestEffectsWorld>(target.GID, source.GID, healPerTick: 5f, duration: 1.5f, period: 1f);

            Tick(1f);
            _tick.Update();
            _apply.Update();

            Tick(1f);
            _tick.Update();
            _apply.Update();

            var afterExpiry = target.Read<CharacteristicComponent<HealthCharacteristic>>().Value;

            Tick(2f);
            _tick.Update();
            _apply.Update();

            Assert.AreEqual(afterExpiry, target.Read<CharacteristicComponent<HealthCharacteristic>>().Value, 0.0001f);
        }
    }
}
