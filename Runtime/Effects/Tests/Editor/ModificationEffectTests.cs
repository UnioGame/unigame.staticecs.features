namespace UniGame.StaticEcs.Features.Tests
{
    using FFS.Libraries.StaticEcs;
    using NUnit.Framework;
    using UniGame.StaticEcs.Tests;
    using UniGame.StaticEcs.Time;

    [TestFixture]
    public sealed class ModificationEffectTests
    {
        private EffectTickSystem<TestEffectsWorld, ModificationEffect<SpeedCharacteristic>> _tick;
        private StaticEcsTestWorld<TestEffectsWorld> _world;

        [SetUp]
        public void SetUp()
        {
            _world = new StaticEcsTestWorld<TestEffectsWorld>();
            var types = _world.Types;
            CharacteristicTypeRegistration.Register<TestEffectsWorld, SpeedCharacteristic>(types);
            EffectTypeRegistration.Register<TestEffectsWorld, ModificationEffect<SpeedCharacteristic>>(types);
            types.Component<ModificationEffectComponent<SpeedCharacteristic>>();

            new EffectsCoreFeature<TestEffectsWorld>().InstallResourcesAndRegisterTypesForTest(
                _world
            );
            new EcsTimeFeature<TestEffectsWorld>().InstallResourcesAndRegisterTypesForTest(
                _world
            );
            new ModifierBackRefFeature<TestEffectsWorld>().InstallResourcesAndRegisterTypesForTest(
                _world
            );
            new CharacteristicFeature<TestEffectsWorld, SpeedCharacteristic>().InstallResourcesAndRegisterTypesForTest(
                _world
            );
            new ModificationEffectFeature<TestEffectsWorld, SpeedCharacteristic>()
                .InstallResourcesAndRegisterTypesForTest(_world);

            _world.Initialize();
            _tick =
                new EffectTickSystem<TestEffectsWorld, ModificationEffect<SpeedCharacteristic>>();
        }

        [TearDown]
        public void TearDown()
        {
            _world?.Dispose();
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
