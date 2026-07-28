namespace UniGame.StaticEcs.Features.Tests
{
    using FFS.Libraries.StaticEcs;
    using NUnit.Framework;
    using UniGame.StaticEcs.Tests;
    using UniGame.StaticEcs.Time;

    /// <summary>
    /// End-to-end smoke for the ability step pipeline. Verifies cast-entity creation,
    /// async leaf timing (Wait), composite advance (Sequence), and synchronous leaf execution
    /// path leading to cast-entity destruction + AbilityCompletedEvent.
    /// </summary>
    [TestFixture]
    public sealed class AbilityCastPipelineTests
    {
        private static readonly AbilityId WaitOnly = new(11);
        private static readonly AbilityId WaitChain = new(12);

        private AbilityCastSystem<TestAbilityWorld> _castSystem;
        private AbilityWaitSystem<TestAbilityWorld> _waitSystem;
        private AbilityStepProgressionSystem<TestAbilityWorld> _progressionSystem;
        private StaticEcsTestWorld<TestAbilityWorld> _world;

        [SetUp]
        public void SetUp()
        {
            _world = new StaticEcsTestWorld<TestAbilityWorld>();
            new EcsTimeFeature<TestAbilityWorld>().InstallResourcesAndRegisterTypesForTest(
                _world
            );
            new AbilityFeature<TestAbilityWorld>().InstallResourcesAndRegisterTypesForTest(
                _world
            );
            _world.Initialize();

            _castSystem = new AbilityCastSystem<TestAbilityWorld>();
            _waitSystem = new AbilityWaitSystem<TestAbilityWorld>();
            _progressionSystem = new AbilityStepProgressionSystem<TestAbilityWorld>();
            _castSystem.Init();
            _progressionSystem.Init();
        }

        [TearDown]
        public void TearDown()
        {
            _world?.TerminateLifeTime();
            _progressionSystem.Destroy();
            _castSystem.Destroy();
            _world?.Dispose();
        }

        private static void Tick(float dt)
        {
            ref var time = ref World<TestAbilityWorld>.GetResource<EcsTime>();
            time.DeltaTime = dt;
            time.Now += dt;
        }

        private void RunSystems()
        {
            _castSystem.Update();
            _waitSystem.Update();
            _progressionSystem.Update();
        }

        [Test]
        public void Cast_WithSingleWaitLeaf_CompletesAfterDuration()
        {
            var registry = World<TestAbilityWorld>.GetResource<AbilityRegistry<TestAbilityWorld>>();
            registry.Register(new AbilityDefinition(WaitOnly), new WaitStepConfig(0.5f));

            var caster = World<TestAbilityWorld>.NewEntity<Default>();
            AbilityOperations.Equip<TestAbilityWorld>(caster.GID, WaitOnly);
            Assert.IsTrue(AbilityOperations.TryStartCast<TestAbilityWorld>(caster.GID, WaitOnly));

            // Frame 1: AbilityCastSystem spawns cast-entity (status armed). Progression descends
            // into root and activates Wait, which goes Running.
            Tick(0.0001f);
            RunSystems();
            Assert.IsTrue(
                caster.Has<AbilityActiveCastComponent>(),
                "active-cast ref must be set after spawn"
            );
            var castGid = caster.Read<AbilityActiveCastComponent>().Cast;
            Assert.IsTrue(castGid.TryUnpack<TestAbilityWorld>(out var castEntity));
            Assert.IsTrue(castEntity.Has<AbilityWaitComponent>(), "wait state must be armed");
            Assert.IsTrue(castEntity.Has<AbilityCurrentStepComponent>());

            // Half a tick — still waiting.
            Tick(0.2f);
            RunSystems();
            Assert.IsTrue(caster.Has<AbilityActiveCastComponent>());

            // Drain remaining time — wait expires, progression terminates the cast.
            Tick(0.4f);
            RunSystems();

            Assert.IsFalse(
                caster.Has<AbilityActiveCastComponent>(),
                "active-cast ref must be cleared on completion"
            );
            Assert.IsFalse(
                castGid.TryUnpack<TestAbilityWorld>(out _),
                "cast-entity must be destroyed"
            );
        }

        [Test]
        public void Cast_WithSequenceWaitWaitWait_AdvancesThroughChildren()
        {
            var registry = World<TestAbilityWorld>.GetResource<AbilityRegistry<TestAbilityWorld>>();
            registry.Register(
                new AbilityDefinition(WaitChain),
                new SequenceStepConfig(
                    new IAbilityStepConfig[]
                    {
                        new WaitStepConfig(0.2f),
                        new WaitStepConfig(0.2f),
                        new WaitStepConfig(0.2f),
                    }
                )
            );

            var caster = World<TestAbilityWorld>.NewEntity<Default>();
            AbilityOperations.Equip<TestAbilityWorld>(caster.GID, WaitChain);
            AbilityOperations.TryStartCast<TestAbilityWorld>(caster.GID, WaitChain);

            var stepReceiver =
                World<TestAbilityWorld>.RegisterEventReceiver<AbilityStepCompletedEvent>();
            var completedReceiver =
                World<TestAbilityWorld>.RegisterEventReceiver<AbilityCompletedEvent>();
            try
            {
                Tick(0.0001f);
                RunSystems();

                for (var i = 0; i < 6; i++)
                {
                    Tick(0.1f);
                    RunSystems();
                }

                Assert.IsFalse(
                    caster.Has<AbilityActiveCastComponent>(),
                    "sequence must finish after all 3 waits"
                );

                var stepCompletions = 0;
                foreach (var e in stepReceiver)
                {
                    if (e.Value.Kind == AbilityStepKind.Wait)
                        stepCompletions++;
                }
                Assert.AreEqual(3, stepCompletions);

                AbilityCompletedReason? lastReason = null;
                foreach (var e in completedReceiver)
                {
                    lastReason = e.Value.Reason;
                }
                Assert.AreEqual(AbilityCompletedReason.Success, lastReason);
            }
            finally
            {
                World<TestAbilityWorld>.DeleteEventReceiver(ref stepReceiver);
                World<TestAbilityWorld>.DeleteEventReceiver(ref completedReceiver);
            }
        }

        [Test]
        public void Cancel_DestroysCastEntity_AndEmitsCancelledEvent()
        {
            var registry = World<TestAbilityWorld>.GetResource<AbilityRegistry<TestAbilityWorld>>();
            registry.Register(new AbilityDefinition(WaitOnly), new WaitStepConfig(5f));

            var caster = World<TestAbilityWorld>.NewEntity<Default>();
            AbilityOperations.Equip<TestAbilityWorld>(caster.GID, WaitOnly);
            AbilityOperations.TryStartCast<TestAbilityWorld>(caster.GID, WaitOnly);

            Tick(0.0001f);
            RunSystems();
            Assert.IsTrue(caster.Has<AbilityActiveCastComponent>());
            var castGid = caster.Read<AbilityActiveCastComponent>().Cast;

            var receiver = World<TestAbilityWorld>.RegisterEventReceiver<AbilityCompletedEvent>();
            try
            {
                Assert.IsTrue(AbilityOperations.Cancel<TestAbilityWorld>(caster.GID));
                Assert.IsFalse(caster.Has<AbilityActiveCastComponent>());
                Assert.IsFalse(castGid.TryUnpack<TestAbilityWorld>(out _), "cast-entity destroyed");

                var cancelled = 0;
                foreach (var e in receiver)
                {
                    if (e.Value.Reason == AbilityCompletedReason.Cancelled)
                        cancelled++;
                }
                Assert.AreEqual(1, cancelled);
            }
            finally
            {
                World<TestAbilityWorld>.DeleteEventReceiver(ref receiver);
            }
        }
    }
}
