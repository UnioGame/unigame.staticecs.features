namespace UniGame.StaticEcs.Features
{
    using System;

    /// <summary>Converts timer configuration and scaled steps to whole milliseconds.</summary>
    public static class TimerMillisecondOperations
    {
        /// <summary>Rounds a seconds setting to whole milliseconds within the uint range.</summary>
        public static uint FromSeconds(float seconds)
        {
            if (!(seconds > 0f))
                return 0u;
            if (float.IsInfinity(seconds) || seconds >= uint.MaxValue / 1000f)
                return uint.MaxValue;

            return (uint)Math.Round((decimal)seconds * 1000m,
                0, MidpointRounding.AwayFromZero);
        }

        /// <summary>Returns whole milliseconds and carries the submillisecond remainder.</summary>
        public static uint TakeStep(float seconds, ref float fractionalMilliseconds)
        {
            var milliseconds = Math.Max(0f, seconds) * 1000f +
                               fractionalMilliseconds;
            if (milliseconds >= uint.MaxValue)
            {
                fractionalMilliseconds = 0f;
                return uint.MaxValue;
            }

            var wholeMilliseconds = (uint)milliseconds;
            fractionalMilliseconds = milliseconds - wholeMilliseconds;
            return wholeMilliseconds;
        }
    }
}
