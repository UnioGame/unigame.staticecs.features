using System;

namespace unigame.staticecs.features {
    /// <summary>
    /// Stable integer identifier for an ability definition. Use a project-wide constants class
    /// or generated enum to map authored names to ids; the registry stores definitions keyed by
    /// this struct.
    /// </summary>
    public readonly struct AbilityId : IEquatable<AbilityId> {
        public readonly int Value;

        public AbilityId(int value) {
            Value = value;
        }

        public bool Equals(AbilityId other) => Value == other.Value;
        public override bool Equals(object obj) => obj is AbilityId other && Equals(other);
        public override int GetHashCode() => Value;
        public override string ToString() => $"AbilityId({Value})";

        public static bool operator ==(AbilityId a, AbilityId b) => a.Value == b.Value;
        public static bool operator !=(AbilityId a, AbilityId b) => a.Value != b.Value;

        public static implicit operator AbilityId(int value) => new AbilityId(value);
        public static implicit operator int(AbilityId id) => id.Value;
    }
}
