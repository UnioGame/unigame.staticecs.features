namespace UniGame.StaticEcs.Features.Tests
{
    using FFS.Libraries.StaticEcs;
    using NUnit.Framework;
    using UniGame.StaticEcs.Tests;
    using UniGame.StaticEcs.Time;

    [TestFixture]
    public sealed class ManaRegenSystemTests
    {
        private const float StartMana = 40f;
        private const float MaxMana = 100f;
        private const float RegenPerSecond = 2f;

        [Test]
        public void ManaGrowsByRegenerationPerSecondOfTime()
        {
            using var world = CreateWorld(deltaTime: 0.5f);
            var entity = NewRegenerating();

            new ManaRegenSystem<ManaRegenTestWorld>().Update();

            Assert.That(ManaOf(entity), Is.EqualTo(StartMana + RegenPerSecond * 0.5f).Within(0.001f));
        }

        [Test]
        public void ManaFollowsTickStepRegardlessOfFrameTime()
        {
            using var world = CreateWorld(deltaTime: 0f);
            var timeConfig = new EcsTimeConfig { UpdateStepSeconds = 0.05f };
            World<ManaRegenTestWorld>.SetResource(timeConfig);
            var entity = NewRegenerating();
            var timeSystem = new EcsTimeUpdateSystem<ManaRegenTestWorld>();
            var regenSystem = new ManaRegenSystem<ManaRegenTestWorld>();

            for (var tick = 0; tick < 200; tick++)
            {
                timeSystem.Update();
                regenSystem.Update();
            }

            Assert.That(ManaOf(entity), Is.EqualTo(StartMana + RegenPerSecond * 10f).Within(0.01f));
        }

        [Test]
        public void ManaIsClampedToMaximum()
        {
            using var world = CreateWorld(deltaTime: 1f);
            var entity = NewRegenerating(mana: MaxMana - 0.5f);

            new ManaRegenSystem<ManaRegenTestWorld>().Update();

            Assert.That(ManaOf(entity), Is.EqualTo(MaxMana));
        }

        [Test]
        public void PausedTimeDoesNotRegenerate()
        {
            using var world = CreateWorld(deltaTime: 0f);
            var entity = NewRegenerating();

            new ManaRegenSystem<ManaRegenTestWorld>().Update();

            Assert.That(ManaOf(entity), Is.EqualTo(StartMana));
        }

        [Test]
        public void BlockedEntityDoesNotRegenerateUntilTheTagIsRemoved()
        {
            using var world = CreateWorld(deltaTime: 1f);
            var entity = NewRegenerating();
            var system = new ManaRegenSystem<ManaRegenTestWorld>();

            entity.Set<ManaRegenBlockedTag>();
            system.Update();
            Assert.That(ManaOf(entity), Is.EqualTo(StartMana));

            entity.Delete<ManaRegenBlockedTag>();
            system.Update();
            Assert.That(ManaOf(entity), Is.GreaterThan(StartMana));
        }

        [Test]
        public void EntityWithoutRegenerationKeepsItsMana()
        {
            using var world = CreateWorld(deltaTime: 1f);
            var entity = NewRegenerating();
            entity.Delete<CharacteristicComponent<ManaRegenCharacteristic>>();

            new ManaRegenSystem<ManaRegenTestWorld>().Update();

            Assert.That(ManaOf(entity), Is.EqualTo(StartMana));
        }

        private static StaticEcsTestWorld<ManaRegenTestWorld> CreateWorld(float deltaTime)
        {
            var world = new StaticEcsTestWorld<ManaRegenTestWorld>();
            CharacteristicTypeRegistration.Register<ManaRegenTestWorld, ManaCharacteristic>(world.Types);
            CharacteristicTypeRegistration.Register<ManaRegenTestWorld, ManaRegenCharacteristic>(world.Types);
            world.Types.Tag<ManaRegenBlockedTag>();
            world.Initialize();

            var time = EcsTime.Default();
            time.DeltaTime = deltaTime;
            World<ManaRegenTestWorld>.SetResource(time);
            return world;
        }

        private static World<ManaRegenTestWorld>.Entity NewRegenerating(float mana = StartMana)
        {
            var entity = World<ManaRegenTestWorld>.NewEntity<Default>();
            entity.Set(new CharacteristicComponent<ManaCharacteristic>(mana, 0f, MaxMana, mana));
            entity.Set(new CharacteristicComponent<ManaRegenCharacteristic>(
                RegenPerSecond,
                0f,
                RegenPerSecond,
                RegenPerSecond));
            return entity;
        }

        private static float ManaOf(World<ManaRegenTestWorld>.Entity entity)
        {
            return entity.Read<CharacteristicComponent<ManaCharacteristic>>().Value;
        }

        private struct ManaRegenTestWorld : IWorldType
        {
        }
    }
}
