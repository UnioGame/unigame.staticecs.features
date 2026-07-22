namespace UniGame.StaticEcs.Features.Tests
{
    using FFS.Libraries.StaticEcs;
    using NUnit.Framework;
    using UniGame.StaticEcs.Time;

    [TestFixture]
    public sealed class ModificationEffectTests
    {
        private EffectTickSystem<TestEffectsWorld, ModificationEffect<SpeedCharacteristic>> _tick;

        [SetUp]
        public void SetUp()
        {
            World<TestEffectsWorld>.Create(WorldConfig.Default());

            new EcsTimeFeature<TestEffectsWorld>(registerFixed: false).RegisterTypes(
                World<TestEffectsWorld>.Types()
            );
            new ModifierBackRefFeature<TestEffectsWorld>().RegisterTypes(
                World<TestEffectsWorld>.Types()
            );
            new CharacteristicFeature<TestEffectsWorld, SpeedCharacteristic>().RegisterTypes(
                World<TestEffectsWorld>.Types()
            );
            new ModificationEffectFeature<TestEffectsWorld, SpeedCharacteristic>(
                registerTickSystem: false
            ).RegisterTypes(World<TestEffectsWorld>.Types());

            World<TestEffectsWorld>.Initialize();
            _tick =
                new EffectTickSystem<TestEffectsWorld, ModificationEffect<SpeedCharacteristic>>();
        }

        [TearDown]
        public void TearDown()
        {
            if (World<TestEffectsWorld>.Status != WorldStatus.NotCreated)
            {
                World<TestEffectsWorld>.Destroy();
            }
        }

        private static void Tick(float dt)
        {
            ref var time = ref World<TestEffectsWorld>.GetResource<EcsTime>();
            time.DeltaTime = dt;
        }

        [Test]
        public void Apply_InstallsModifier_OnExpire_Reverts()
        {
            var source = World<TestEffectsWorld>.NewEntity<Default>();
            var target = World<TestEffectsWorld>.NewEntity<Default>();
            target.Set(CharacteristicComponent<SpeedCharacteristic>.Create(10f, 0f, 1000f));

            ModificationEffectOperations.Apply<TestEffectsWorld, SpeedCharacteristic>(
                target.GID,
                source.GID,
                CharacteristicModifierOp.Mul,
                2f,
                duration: 1f
            );

            Assert.AreEqual(
                20f,
                target.Read<CharacteristicComponent<SpeedCharacteristic>>().Value,
                0.0001f
            );

            Tick(2f);
            _tick.Update();

            Assert.AreEqual(
                10f,
                target.Read<CharacteristicComponent<SpeedCharacteristic>>().Value,
                0.0001f
            );
        }

        [Test]
        public void Reapply_OverwritesModifier_FromSameSource()
        {
            var source = World<TestEffectsWorld>.NewEntity<Default>();
            var target = World<TestEffectsWorld>.NewEntity<Default>();
            target.Set(CharacteristicComponent<SpeedCharacteristic>.Create(10f, 0f, 1000f));

            ModificationEffectOperations.Apply<TestEffectsWorld, SpeedCharacteristic>(
                target.GID,
                source.GID,
                CharacteristicModifierOp.Mul,
                2f,
                duration: 5f
            );
            Assert.AreEqual(
                20f,
                target.Read<CharacteristicComponent<SpeedCharacteristic>>().Value,
                0.0001f
            );

            ModificationEffectOperations.Apply<TestEffectsWorld, SpeedCharacteristic>(
                target.GID,
                source.GID,
                CharacteristicModifierOp.Mul,
                3f,
                duration: 5f
            );
            Assert.AreEqual(
                30f,
                target.Read<CharacteristicComponent<SpeedCharacteristic>>().Value,
                0.0001f
            );
        }

        [Test]
        public void SourceDestroyed_RemovesModifier()
        {
            var source = World<TestEffectsWorld>.NewEntity<Default>();
            var target = World<TestEffectsWorld>.NewEntity<Default>();
            target.Set(CharacteristicComponent<SpeedCharacteristic>.Create(10f, 0f, 1000f));

            ModificationEffectOperations.Apply<TestEffectsWorld, SpeedCharacteristic>(
                target.GID,
                source.GID,
                CharacteristicModifierOp.Mul,
                2f,
                duration: 100f
            );
            Assert.AreEqual(
                20f,
                target.Read<CharacteristicComponent<SpeedCharacteristic>>().Value,
                0.0001f
            );

            source.Destroy();

            Tick(0.1f);
            _tick.Update();

            Assert.AreEqual(
                10f,
                target.Read<CharacteristicComponent<SpeedCharacteristic>>().Value,
                0.0001f
            );
        }
    }
}
