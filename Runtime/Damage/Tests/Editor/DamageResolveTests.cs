namespace UniGame.StaticEcs.Features.Tests
{
    using FFS.Libraries.StaticEcs;
    using NUnit.Framework;
    using UniGame.StaticEcs.Tests;

    [TestFixture]
    public sealed class DamageResolveTests
    {
        private FakeDamageRng _rng;
        private StaticEcsTestWorld<TestDamageWorld> _world;

        [SetUp]
        public void SetUp()
        {
            _world = new StaticEcsTestWorld<TestDamageWorld>();
            var types = World<TestDamageWorld>.Types();
            CharacteristicTypeRegistration.Register<TestDamageWorld, HealthCharacteristic>(types);
            CharacteristicTypeRegistration.Register<TestDamageWorld, ShieldCharacteristic>(types);
            CharacteristicTypeRegistration.Register<TestDamageWorld, BlockChanceCharacteristic>(types);
            CharacteristicTypeRegistration.Register<TestDamageWorld, DodgeChanceCharacteristic>(types);
            CharacteristicTypeRegistration.Register<TestDamageWorld, ArmorResistCharacteristic>(types);
            CharacteristicTypeRegistration.Register<TestDamageWorld, CriticalChanceCharacteristic>(types);
            CharacteristicTypeRegistration.Register<TestDamageWorld, CriticalMultiplierCharacteristic>(types);
            _rng = new FakeDamageRng();
            World<TestDamageWorld>.SetResource<IDamageRng>(_rng);
            new DamageFeature<TestDamageWorld>().InstallResourcesAndRegisterTypesForTest(_world);
            _world.Initialize();
        }

        [TearDown]
        public void TearDown()
        {
            _world?.Dispose();
        }

        [Test]
        public void Resolve_AppliesAmountToHealthAndReportsIt()
        {
            var source = World<TestDamageWorld>.NewEntity<Default>();
            var target = CreateTarget(100f);

            var resolution = DamageOperations.Resolve<TestDamageWorld>(Damage(source.GID, target.GID, 30f));

            Assert.That(resolution.IsApplied, Is.True);
            Assert.That(resolution.IsCritical, Is.False);
            Assert.That(resolution.AppliedAmount, Is.EqualTo(30f));
            Assert.That(Health(target), Is.EqualTo(70f));
        }

        [Test]
        public void Resolve_RolledCritical_AmplifiesWithoutRolling()
        {
            var source = World<TestDamageWorld>.NewEntity<Default>();
            var target = CreateTarget(100f);
            _rng.NextRoll = false;

            var damage = Damage(source.GID, target.GID, 20f);
            damage.ForceCritical = true;
            damage.CriticalRolled = true;
            var resolution = DamageOperations.Resolve<TestDamageWorld>(damage);

            Assert.That(resolution.IsCritical, Is.True);
            Assert.That(Health(target), Is.EqualTo(100f - 20f * CriticalFilter<TestDamageWorld>.DefaultMultiplier));
        }

        [Test]
        public void Resolve_RolledNonCritical_SkipsTheFilterRoll()
        {
            var source = World<TestDamageWorld>.NewEntity<Default>();
            source.Set(CharacteristicComponent<CriticalChanceCharacteristic>.Create(1f, 0f, 1f));
            var target = CreateTarget(100f);
            _rng.NextRoll = true;

            var damage = Damage(source.GID, target.GID, 20f);
            damage.CriticalRolled = true;
            var resolution = DamageOperations.Resolve<TestDamageWorld>(damage);

            Assert.That(resolution.IsCritical, Is.False);
            Assert.That(Health(target), Is.EqualTo(80f));
        }

        [Test]
        public void Resolve_NotRolled_RollsTheFilterChance()
        {
            var source = World<TestDamageWorld>.NewEntity<Default>();
            source.Set(CharacteristicComponent<CriticalChanceCharacteristic>.Create(0.5f, 0f, 1f));
            var target = CreateTarget(100f);
            _rng.NextRoll = true;

            var resolution = DamageOperations.Resolve<TestDamageWorld>(Damage(source.GID, target.GID, 20f));

            Assert.That(resolution.IsCritical, Is.True);
        }

        [Test]
        public void Resolve_Dodged_CancelsAndLeavesHealth()
        {
            var source = World<TestDamageWorld>.NewEntity<Default>();
            var target = CreateTarget(100f);
            target.Set(CharacteristicComponent<DodgeChanceCharacteristic>.Create(1f, 0f, 1f));
            _rng.NextRoll = true;

            var resolution = DamageOperations.Resolve<TestDamageWorld>(Damage(source.GID, target.GID, 20f));

            Assert.That(resolution.CancelReason, Is.EqualTo(DamageCancelReason.Dodged));
            Assert.That(resolution.IsApplied, Is.False);
            Assert.That(Health(target), Is.EqualTo(100f));
        }

        [Test]
        public void Resolve_Area_IgnoresDodgeAndNeverCrits()
        {
            var source = World<TestDamageWorld>.NewEntity<Default>();
            source.Set(CharacteristicComponent<CriticalChanceCharacteristic>.Create(1f, 0f, 1f));
            var target = CreateTarget(100f);
            target.Set(CharacteristicComponent<DodgeChanceCharacteristic>.Create(1f, 0f, 1f));
            _rng.NextRoll = true;

            var damage = Damage(source.GID, target.GID, 5f);
            damage.AttackType = AttackType.Area;
            damage.ForceCritical = true;
            var resolution = DamageOperations.Resolve<TestDamageWorld>(damage);

            Assert.That(resolution.CancelReason, Is.EqualTo(DamageCancelReason.None));
            Assert.That(resolution.IsCritical, Is.False);
            Assert.That(Health(target), Is.EqualTo(95f));
        }

        [Test]
        public void Resolve_Area_IgnoresBlock()
        {
            var source = World<TestDamageWorld>.NewEntity<Default>();
            var target = CreateTarget(100f);
            target.Set<BlockableTag>();
            target.Set(CharacteristicComponent<BlockChanceCharacteristic>.Create(1f, 0f, 1f));
            _rng.NextRoll = true;

            var damage = Damage(source.GID, target.GID, 5f);
            damage.AttackType = AttackType.Area;
            var resolution = DamageOperations.Resolve<TestDamageWorld>(damage);

            Assert.That(resolution.CancelReason, Is.EqualTo(DamageCancelReason.None));
            Assert.That(Health(target), Is.EqualTo(95f));
        }

        [TestCase(AttackType.Hit)]
        [TestCase(AttackType.Projectile)]
        [TestCase(AttackType.AreaHit)]
        public void Resolve_RollingAttackTypes_CanBeDodged(AttackType attackType)
        {
            var source = World<TestDamageWorld>.NewEntity<Default>();
            var target = CreateTarget(100f);
            target.Set(CharacteristicComponent<DodgeChanceCharacteristic>.Create(1f, 0f, 1f));
            _rng.NextRoll = true;

            var damage = Damage(source.GID, target.GID, 20f);
            damage.AttackType = attackType;
            var resolution = DamageOperations.Resolve<TestDamageWorld>(damage);

            Assert.That(resolution.CancelReason, Is.EqualTo(DamageCancelReason.Dodged));
            Assert.That(Health(target), Is.EqualTo(100f));
        }

        [Test]
        public void Resolve_AreaHit_RollsBlockAndCritical()
        {
            var source = World<TestDamageWorld>.NewEntity<Default>();
            source.Set(CharacteristicComponent<CriticalChanceCharacteristic>.Create(1f, 0f, 1f));
            var blocker = CreateTarget(100f);
            blocker.Set<BlockableTag>();
            blocker.Set(CharacteristicComponent<BlockChanceCharacteristic>.Create(1f, 0f, 1f));
            var plain = CreateTarget(100f);
            _rng.NextRoll = true;

            var blockedHit = Damage(source.GID, blocker.GID, 10f);
            blockedHit.AttackType = AttackType.AreaHit;
            var blocked = DamageOperations.Resolve<TestDamageWorld>(blockedHit);

            var criticalHit = Damage(source.GID, plain.GID, 10f);
            criticalHit.AttackType = AttackType.AreaHit;
            var critical = DamageOperations.Resolve<TestDamageWorld>(criticalHit);

            Assert.That(blocked.CancelReason, Is.EqualTo(DamageCancelReason.Blocked));
            Assert.That(critical.IsCritical, Is.True);
        }

        [Test]
        public void Resolve_Area_StillAppliesDamage()
        {
            var source = World<TestDamageWorld>.NewEntity<Default>();
            var target = CreateTarget(100f);
            _rng.NextRoll = true;

            var damage = Damage(source.GID, target.GID, 10f);
            damage.AttackType = AttackType.Area;
            var resolution = DamageOperations.Resolve<TestDamageWorld>(damage);

            Assert.That(resolution.IsApplied, Is.True);
            Assert.That(Health(target), Is.EqualTo(90f));
        }

        [TestCase(AttackType.Hit, true)]
        [TestCase(AttackType.Projectile, true)]
        [TestCase(AttackType.Area, false)]
        [TestCase(AttackType.AreaHit, true)]
        public void RollsHitChecks_OnlyAreaSkipsThem(AttackType attackType, bool expected)
        {
            Assert.That(attackType.RollsHitChecks(), Is.EqualTo(expected));
        }

        [Test]
        public void RollCritical_UsesSourceChance()
        {
            var source = World<TestDamageWorld>.NewEntity<Default>();
            source.Set(CharacteristicComponent<CriticalChanceCharacteristic>.Create(0.3f, 0f, 1f));

            _rng.NextRoll = true;
            Assert.That(DamageOperations.RollCritical<TestDamageWorld>(source.GID), Is.True);

            _rng.NextRoll = false;
            Assert.That(DamageOperations.RollCritical<TestDamageWorld>(source.GID), Is.False);
        }

        [Test]
        public void RollCritical_WithoutChance_NeverRolls()
        {
            var source = World<TestDamageWorld>.NewEntity<Default>();
            _rng.NextRoll = true;

            Assert.That(DamageOperations.RollCritical<TestDamageWorld>(source.GID), Is.False);
        }

        private static IncomingDamageEvent Damage(EntityGID source, EntityGID target, float amount)
        {
            return new IncomingDamageEvent
            {
                Source = source,
                Target = target,
                Amount = amount,
                Type = DamageType.Physical,
            };
        }

        private static World<TestDamageWorld>.Entity CreateTarget(float health)
        {
            var target = World<TestDamageWorld>.NewEntity<Default>();
            target.Set(CharacteristicComponent<HealthCharacteristic>.Create(health, 0f, health));
            return target;
        }

        private static float Health(World<TestDamageWorld>.Entity entity)
        {
            return entity.Read<CharacteristicComponent<HealthCharacteristic>>().Value;
        }
    }
}
