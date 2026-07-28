namespace UniGame.StaticEcs.Features
{
    using System;

    /// <summary>Uniquely identifies one accepted view request.</summary>
    [Serializable]
    public readonly struct ViewKey : IEquatable<ViewKey>
    {
        /// <summary>Represents an invalid view key.</summary>
        public static readonly ViewKey Invalid = default;

        /// <summary>Creates a key from its numeric value.</summary>
        public ViewKey(ulong value)
        {
            Value = value;
        }

        /// <summary>Gets the numeric key value.</summary>
        public ulong Value { get; }

        /// <summary>Gets whether the key can identify a request.</summary>
        public bool IsValid => Value != 0;

        /// <inheritdoc />
        public bool Equals(ViewKey other) => Value == other.Value;

        /// <inheritdoc />
        public override bool Equals(object obj) => obj is ViewKey other && Equals(other);

        /// <inheritdoc />
        public override int GetHashCode() => Value.GetHashCode();

        /// <summary>Compares two view keys.</summary>
        public static bool operator ==(ViewKey left, ViewKey right) => left.Equals(right);

        /// <summary>Compares two view keys.</summary>
        public static bool operator !=(ViewKey left, ViewKey right) => !left.Equals(right);

        /// <inheritdoc />
        public override string ToString() => Value.ToString();
    }
}
