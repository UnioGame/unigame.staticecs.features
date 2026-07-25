namespace UniGame.StaticEcs.Features
{
    using System;

    /// <summary>
    /// Stable integer identifier for an effect type. Allocated once per process by
    /// <see cref="EffectIdRegistry"/> when the corresponding effect feature initializes for a
    /// given <c>TEffect</c>; rosters and UI store this id instead of <see cref="Type"/>.
    /// </summary>
    [Serializable]
    public readonly struct EffectId : IEquatable<EffectId>
    {
        public const int InvalidValue = 0;

        public readonly int Value;

        public EffectId(int value)
        {
            Value = value;
        }

        public bool IsValid => Value > 0;

        public bool Equals(EffectId other) => Value == other.Value;

        public override bool Equals(object obj) => obj is EffectId other && Equals(other);

        public override int GetHashCode() => Value;

        public override string ToString() => $"EffectId({Value})";
    }
}
