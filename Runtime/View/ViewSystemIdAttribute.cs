namespace UniGame.StaticEcs.Features
{
    using System;

    /// <summary>Stores the exact View System key for a generated enum member.</summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public sealed class ViewSystemIdAttribute : Attribute
    {
        /// <summary>Creates a mapping from an enum member to a View System key.</summary>
        public ViewSystemIdAttribute(string value)
        {
            Value = value;
        }

        /// <summary>Exact case-insensitive View System key.</summary>
        public string Value { get; }
    }
}
