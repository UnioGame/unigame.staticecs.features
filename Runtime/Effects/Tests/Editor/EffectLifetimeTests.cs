namespace UniGame.StaticEcs.Features.Tests
{
    using NUnit.Framework;

    [TestFixture]
    public sealed class EffectLifetimeTests
    {
        [Test]
        public void Merge_WithoutAPreviousState_StartsTheEffect()
        {
            var rules = Rules(maxStacks: 3, refresh: true, accumulate: true);
            var empty = State(stacks: 0, timeLeft: 9f, chargesLeft: 9, period: 9f, periodLeft: 9f, delayLeft: 9f);

            var next = EffectLifetime.Merge(in empty, Input(duration: 3f, charges: 2, period: 0.5f, delay: 1f), in rules);

            Assert.AreEqual(1, next.Stacks);
            Assert.AreEqual(3f, next.TimeLeft, 0.0001f);
            Assert.AreEqual(2, next.ChargesLeft);
            Assert.AreEqual(0.5f, next.Period, 0.0001f);
            Assert.AreEqual(0.5f, next.PeriodLeft, 0.0001f, "a new effect starts a full period");
            Assert.AreEqual(1f, next.DelayLeft, 0.0001f);
        }

        [Test]
        public void Merge_WithoutAPeriod_StartsWithoutAPhase()
        {
            var rules = Rules();
            var empty = State(stacks: 0);

            var next = EffectLifetime.Merge(in empty, Input(duration: 3f), in rules);

            Assert.AreEqual(0f, next.PeriodLeft);
        }

        [Test]
        public void Merge_Refresh_OfARunningEffectNeverIntroducesADelay()
        {
            var rules = Rules(refresh: true);
            var running = State(timeLeft: 2f, delayLeft: 0f);

            var next = EffectLifetime.Merge(in running, Input(duration: 5f, delay: 3f), in rules);

            Assert.AreEqual(0f, next.DelayLeft);
        }

        [Test]
        public void Merge_AddsAStackUpToTheCap()
        {
            var rules = Rules(maxStacks: 2);
            var state = State(stacks: 1);

            state = EffectLifetime.Merge(in state, Input(), in rules);
            Assert.AreEqual(2, state.Stacks);

            state = EffectLifetime.Merge(in state, Input(), in rules);
            Assert.AreEqual(2, state.Stacks, "capped by MaxStacks");
        }

        [Test]
        public void Merge_Accumulate_AddsDurationUpToTheCapAndKeepsThePhase()
        {
            var rules = Rules(accumulate: true, maxDuration: 5f);
            var state = State(timeLeft: 2.5f, period: 1f, periodLeft: 0.5f);

            var next = EffectLifetime.Merge(in state, Input(duration: 1f, period: 9f), in rules);
            Assert.AreEqual(3.5f, next.TimeLeft, 0.0001f);
            Assert.AreEqual(1f, next.Period, 0.0001f, "accumulating keeps the period");
            Assert.AreEqual(0.5f, next.PeriodLeft, 0.0001f, "accumulating keeps the activation phase");

            next = EffectLifetime.Merge(in next, Input(duration: 3f), in rules);
            Assert.AreEqual(5f, next.TimeLeft, 0.0001f, "capped by MaxDuration");
        }

        [Test]
        public void Merge_Accumulate_WithoutADurationCapKeepsAdding()
        {
            var rules = Rules(accumulate: true, maxDuration: 0f);
            var state = State(timeLeft: 100f);

            var next = EffectLifetime.Merge(in state, Input(duration: 50f), in rules);

            Assert.AreEqual(150f, next.TimeLeft, 0.0001f);
        }

        [Test]
        public void Merge_Accumulate_AddsChargesUpToTheCapForAChargedEffect()
        {
            var rules = Rules(EffectDurationMode.Charges, accumulate: true, maxCharges: 5);
            var state = State(chargesLeft: 3, timeLeft: 7f);

            var next = EffectLifetime.Merge(in state, Input(charges: 4, duration: 9f), in rules);

            Assert.AreEqual(5, next.ChargesLeft);
            Assert.AreEqual(7f, next.TimeLeft, 0.0001f, "a charged effect leaves the time alone");
        }

        [Test]
        public void Merge_Refresh_TakesTheLargerLifetimeAndRestartsThePeriodAndTheDelay()
        {
            var rules = Rules(refresh: true);
            var state = State(timeLeft: 2f, chargesLeft: 1, period: 1f, periodLeft: 0.2f, delayLeft: 4f);

            var next = EffectLifetime.Merge(in state, Input(duration: 5f, charges: 3, period: 2f, delay: 1f), in rules);

            Assert.AreEqual(5f, next.TimeLeft, 0.0001f);
            Assert.AreEqual(3, next.ChargesLeft);
            Assert.AreEqual(2f, next.Period, 0.0001f);
            Assert.AreEqual(2f, next.PeriodLeft, 0.0001f);
            Assert.AreEqual(1f, next.DelayLeft, 0.0001f);
        }

        [Test]
        public void Merge_Refresh_NeverShortensTheLifetime()
        {
            var rules = Rules(refresh: true);
            var state = State(timeLeft: 5f, chargesLeft: 4);

            var next = EffectLifetime.Merge(in state, Input(duration: 1f, charges: 1), in rules);

            Assert.AreEqual(5f, next.TimeLeft, 0.0001f);
            Assert.AreEqual(4, next.ChargesLeft);
        }

        [Test]
        public void Merge_Refresh_WithoutAPeriodLeavesNoPhase()
        {
            var rules = Rules(refresh: true);
            var state = State(period: 1f, periodLeft: 0.5f);

            var next = EffectLifetime.Merge(in state, Input(period: 0f), in rules);

            Assert.AreEqual(0f, next.Period);
            Assert.AreEqual(0f, next.PeriodLeft);
        }

        [Test]
        public void Merge_AccumulateWinsOverRefresh()
        {
            var rules = Rules(accumulate: true, refresh: true, maxDuration: 10f);
            var state = State(timeLeft: 2f, period: 1f, periodLeft: 0.5f);

            var next = EffectLifetime.Merge(in state, Input(duration: 3f, period: 4f), in rules);

            Assert.AreEqual(5f, next.TimeLeft, 0.0001f);
            Assert.AreEqual(1f, next.Period, 0.0001f);
            Assert.AreEqual(0.5f, next.PeriodLeft, 0.0001f);
        }

        [Test]
        public void Merge_WithoutRefreshAndAccumulate_OnlyAddsTheStack()
        {
            var rules = Rules(maxStacks: 3);
            var state = State(stacks: 1, timeLeft: 2f, chargesLeft: 2, period: 1f, periodLeft: 0.4f, delayLeft: 3f);

            var next = EffectLifetime.Merge(in state, Input(duration: 9f, charges: 9, period: 9f, delay: 9f), in rules);

            Assert.AreEqual(2, next.Stacks);
            Assert.AreEqual(2f, next.TimeLeft, 0.0001f);
            Assert.AreEqual(2, next.ChargesLeft);
            Assert.AreEqual(1f, next.Period, 0.0001f);
            Assert.AreEqual(0.4f, next.PeriodLeft, 0.0001f);
            Assert.AreEqual(3f, next.DelayLeft, 0.0001f);
        }

        [Test]
        public void Merge_DoesNotChangeThePreviousState()
        {
            var rules = Rules(refresh: true);
            var state = State(stacks: 1, timeLeft: 2f);

            EffectLifetime.Merge(in state, Input(duration: 5f), in rules);

            Assert.AreEqual(1, state.Stacks);
            Assert.AreEqual(2f, state.TimeLeft, 0.0001f);
        }

        [Test]
        public void IsExhausted_TimedEffectEndsWhenTheTimeRunsOut()
        {
            Assert.IsFalse(EffectLifetime.IsExhausted(EffectDurationMode.Time, 0.5f, 0));
            Assert.IsTrue(EffectLifetime.IsExhausted(EffectDurationMode.Time, 0f, 9));
            Assert.IsTrue(
                EffectLifetime.IsExhausted(EffectDurationMode.Time, EffectLifetime.TimeEpsilon, 9),
                "the epsilon absorbs float drift");
            Assert.IsTrue(EffectLifetime.IsExhausted(EffectDurationMode.Time, -1f, 9));
        }

        [Test]
        public void IsExhausted_ChargedEffectEndsWhenTheChargesRunOut()
        {
            Assert.IsFalse(EffectLifetime.IsExhausted(EffectDurationMode.Charges, 0f, 1));
            Assert.IsTrue(EffectLifetime.IsExhausted(EffectDurationMode.Charges, 9f, 0));
        }

        [Test]
        public void IsExhausted_InstantEffectIsAlwaysSpent()
        {
            Assert.IsTrue(EffectLifetime.IsExhausted(EffectDurationMode.Instant, 9f, 9));
        }

        private static EffectMergeRules Rules(
            EffectDurationMode mode = EffectDurationMode.Time,
            int maxStacks = 1,
            bool accumulate = false,
            bool refresh = false,
            float maxDuration = 0f,
            int maxCharges = 0)
        {
            return new EffectMergeRules(maxStacks, mode, accumulate, refresh, maxDuration, maxCharges);
        }

        private static EffectLifetimeState State(
            int stacks = 1,
            float timeLeft = 0f,
            int chargesLeft = 0,
            float period = 0f,
            float periodLeft = 0f,
            float delayLeft = 0f)
        {
            return new EffectLifetimeState
            {
                Stacks = stacks,
                TimeLeft = timeLeft,
                ChargesLeft = chargesLeft,
                Period = period,
                PeriodLeft = periodLeft,
                DelayLeft = delayLeft,
            };
        }

        private static EffectLifetimeInput Input(
            float duration = 0f,
            int charges = 0,
            float period = 0f,
            float delay = 0f)
        {
            return new EffectLifetimeInput(duration, charges, period, delay);
        }
    }
}
