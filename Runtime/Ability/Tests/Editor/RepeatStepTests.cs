namespace UniGame.StaticEcs.Features.Tests
{
    using FFS.Libraries.StaticEcs;
    using NUnit.Framework;
    using UniGame.StaticEcs.Time;

    [TestFixture]
    public sealed class RepeatStepTests
    {
        private static readonly AbilityId Ability = new(303);

        private AbilityCastSystem<TestAbilityWorld> _castSystem;
        private AbilityWaitSystem<TestAbilityWorld> _waitSystem;
        private AbilityStepProgressionSystem<TestAbilityWorld> _progressionSystem;

        [SetUp]
        public void SetUp()
        {
            World<TestAbilityWorld>.Create(WorldConfig.Default());
            new EcsTimeFeature<TestAbilityWorld>(registerFixed: false).RegisterTypes(
                World<TestAbilityWorld>.Types()
            );
            new DamageFeature<TestAbilityWorld>(registerApplySystem: false).RegisterTypes(
                World<TestAbilityWorld>.Types()
            );
            new AbilityFeature<TestAbilityWorld>(registerSystems: false).RegisterTypes(
                World<TestAbilityWorld>.Types()
            );
            World<TestAbilityWorld>.Initialize();

            _castSystem = new AbilityCastSystem<TestAbilityWorld>();
            _waitSystem = new AbilityWaitSystem<TestAbilityWorld>();
            _progressionSystem = new AbilityStepProgressionSystem<TestAbilityWorld>();
            _castSystem.Init();
            _progressionSystem.Init();
        }

        [TearDown]
        public void TearDown()
        {
            _castSystem.Destroy();
            _progressionSystem.Destroy();
            if (World<TestAbilityWorld>.Status != WorldStatus.NotCreated)
            {
                World<TestAbilityWorld>.Destroy();
            }
        }

        [Test]
        public void Repeat_RunsBodyMaxIterations()
        {
            Register(
                new RepeatStepConfig(
                    new ApplyDamageStepConfig(1f, AbilityTargetMode.Self),
                    maxIterations: 3
                )
            );
            var caster = StartCast();
            var receiver = World<TestAbilityWorld>.RegisterEventReceiver<IncomingDamageEvent>();
            try
            {
                RunFrame();

                var hits = 0;
                foreach (var _ in receiver)
                {
                    hits++;
                }

                Assert.AreEqual(3, hits);
                Assert.IsFalse(caster.Has<AbilityActiveCastComponent>());
            }
            finally
            {
                World<TestAbilityWorld>.DeleteEventReceiver(ref receiver);
            }
        }

        [Test]
        public void Repeat_StopsWhenConditionFalse()
        {
            Register(
                new RepeatStepConfig(
                    new ApplyDamageStepConfig(1f, AbilityTargetMode.Self),
                    maxIterations: 3,
                    whileCondition: new NeverCondition()
                )
            );
            var caster = StartCast();
            var receiver = World<TestAbilityWorld>.RegisterEventReceiver<IncomingDamageEvent>();
            try
            {
                RunFrame();

                var hits = 0;
                foreach (var _ in receiver)
                {
                    hits++;
                }

                Assert.AreEqual(0, hits);
                Assert.IsFalse(caster.Has<AbilityActiveCastComponent>());
            }
            finally
            {
                World<TestAbilityWorld>.DeleteEventReceiver(ref receiver);
            }
        }

        [Test]
        public void Repeat_FailsWhenBodyFails()
        {
            Register(
                new RepeatStepConfig(
                    new ApplyDamageStepConfig(1f, AbilityTargetMode.PrimaryTarget),
                    maxIterations: 3
                )
            );
            var caster = StartCast();
            var receiver = World<TestAbilityWorld>.RegisterEventReceiver<AbilityCompletedEvent>();
            try
            {
                RunFrame();

                AbilityCompletedReason? reason = null;
                foreach (var e in receiver)
                {
                    reason = e.Value.Reason;
                }

                Assert.AreEqual(AbilityCompletedReason.Cancelled, reason);
                Assert.IsFalse(caster.Has<AbilityActiveCastComponent>());
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

        private static void Tick(float dt)
        {
            ref var time = ref World<TestAbilityWorld>.GetResource<EcsTime>();
            time.DeltaTime = dt;
            time.Now += dt;
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
