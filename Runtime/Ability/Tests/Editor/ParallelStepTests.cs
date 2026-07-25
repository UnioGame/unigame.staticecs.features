namespace UniGame.StaticEcs.Features.Tests
{
    using FFS.Libraries.StaticEcs;
    using NUnit.Framework;
    using UniGame.StaticEcs.Tests;
    using UniGame.StaticEcs.Time;

    [TestFixture]
    public sealed class ParallelStepTests
    {
        private static readonly AbilityId Ability = new(301);

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
            new DamageFeature<TestAbilityWorld>().InstallResourcesAndRegisterTypesForTest(
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
            _castSystem.Destroy();
            _progressionSystem.Destroy();
            _world?.Dispose();
        }

        [Test]
        public void AllSuccess_CompletesWhenAllBranchesComplete()
        {
            Register(
                new ParallelStepConfig(
                    new IAbilityStepConfig[]
                    {
                        new ApplyDamageStepConfig(1f, AbilityTargetMode.Self),
                        new ApplyDamageStepConfig(1f, AbilityTargetMode.Self),
                    }
                )
            );

            var caster = StartCast();
            var receiver = World<TestAbilityWorld>.RegisterEventReceiver<IncomingDamageEvent>();
            try
            {
                RunFrames(3);

                var hits = 0;
                foreach (var _ in receiver)
                {
                    hits++;
                }

                Assert.AreEqual(2, hits);
                Assert.IsFalse(caster.Has<AbilityActiveCastComponent>());
            }
            finally
            {
                World<TestAbilityWorld>.DeleteEventReceiver(ref receiver);
            }
        }

        [Test]
        public void AllSuccess_FailsWhenAnyBranchFails()
        {
            Register(
                new ParallelStepConfig(
                    new IAbilityStepConfig[]
                    {
                        new ApplyDamageStepConfig(1f, AbilityTargetMode.Self),
                        new ApplyDamageStepConfig(1f, AbilityTargetMode.PrimaryTarget),
                    }
                )
            );

            var caster = StartCast();
            RunFrame();
            var parentCast = caster.Read<AbilityActiveCastComponent>().Cast;
            var receiver = World<TestAbilityWorld>.RegisterEventReceiver<AbilityCompletedEvent>();
            try
            {
                RunFrames(3);

                AbilityCompletedReason? parentReason = null;
                foreach (var e in receiver)
                {
                    if (e.Value.CastEntity.Equals(parentCast))
                    {
                        parentReason = e.Value.Reason;
                    }
                }

                Assert.AreEqual(AbilityCompletedReason.Cancelled, parentReason);
                Assert.IsFalse(caster.Has<AbilityActiveCastComponent>());
            }
            finally
            {
                World<TestAbilityWorld>.DeleteEventReceiver(ref receiver);
            }
        }

        [Test]
        public void AnySuccess_CompletesOnFirstSuccess_AndCancelsRemaining()
        {
            Register(
                new ParallelStepConfig(
                    new IAbilityStepConfig[] { new WaitStepConfig(0f), new WaitStepConfig(5f) },
                    ParallelJoinPolicy.AnySuccess,
                    cancelRemainingOnJoin: true
                )
            );

            var caster = StartCast();
            RunFrames(4);

            Assert.IsFalse(caster.Has<AbilityActiveCastComponent>());
            Assert.AreEqual(0, CountBranchCasts());
        }

        [Test]
        public void ExternalCancel_RecursivelyDestroysParallelBranches()
        {
            Register(
                new ParallelStepConfig(
                    new IAbilityStepConfig[]
                    {
                        new WaitStepConfig(5f),
                        new WaitStepConfig(10f),
                    }));

            var caster = StartCast();
            RunFrame();
            var rootCast = caster.Read<AbilityActiveCastComponent>().Cast;
            Assert.AreEqual(2, CountBranchCasts());
            var receiver =
                World<TestAbilityWorld>.RegisterEventReceiver<AbilityCompletedEvent>();
            try
            {
                Assert.IsTrue(
                    AbilityOperations.Cancel<TestAbilityWorld>(caster.GID));

                Assert.IsFalse(caster.Has<AbilityActiveCastComponent>());
                Assert.IsFalse(rootCast.TryUnpack<TestAbilityWorld>(out _));
                Assert.AreEqual(0, CountBranchCasts());

                var rootCompletions = 0;
                foreach (var e in receiver)
                {
                    if (e.Value.CastEntity.Equals(rootCast))
                    {
                        rootCompletions++;
                        Assert.AreEqual(
                            AbilityCompletedReason.Cancelled,
                            e.Value.Reason);
                    }
                }

                Assert.AreEqual(1, rootCompletions);
            }
            finally
            {
                World<TestAbilityWorld>.DeleteEventReceiver(ref receiver);
            }
        }

        private static void Register(IAbilityStepConfig root)
        {
            var registry = World<TestAbilityWorld>.GetResource<AbilityRegistry<TestAbilityWorld>>();
            registry.Register(new AbilityDefinition(Ability), root);
        }

        private static World<TestAbilityWorld>.Entity StartCast()
        {
            var caster = World<TestAbilityWorld>.NewEntity<Default>();
            AbilityOperations.Equip<TestAbilityWorld>(caster.GID, Ability);
            Assert.IsTrue(AbilityOperations.TryStartCast<TestAbilityWorld>(caster.GID, Ability));
            return caster;
        }

        private static int CountBranchCasts()
        {
            var count = 0;
            foreach (
                var _ in World<TestAbilityWorld>
                    .Query<All<AbilityCastComponent, AbilityBranchSubcastTag>>()
                    .Entities()
            )
            {
                count++;
            }
            return count;
        }

        private static void Tick(float dt)
        {
            ref var time = ref World<TestAbilityWorld>.GetResource<EcsTime>();
            time.DeltaTime = dt;
            time.Now += dt;
        }

        private void RunFrames(int count)
        {
            for (var i = 0; i < count; i++)
            {
                RunFrame();
            }
        }

        private void RunFrame()
        {
            Tick(0.0001f);
            _castSystem.Update();
            _waitSystem.Update();
            _progressionSystem.Update();
        }
    }
}
