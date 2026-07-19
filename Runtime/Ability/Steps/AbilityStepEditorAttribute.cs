using System;

namespace UniGame.StaticEcs.Features {
    /// <summary>
    /// Optional editor metadata for an <see cref="IAbilityStepConfig"/> type.
    /// Provides a stable display name and palette category without affecting runtime execution.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public sealed class AbilityStepEditorAttribute : Attribute {
        /// <summary>
        /// Creates metadata for a custom or built-in ability step config type.
        /// </summary>
        public AbilityStepEditorAttribute(string displayName, string category = null) {
            DisplayName = displayName;
            Category = category;
        }

        /// <summary>
        /// Human-readable node title used by graph and palette surfaces.
        /// </summary>
        public string DisplayName { get; }

        /// <summary>
        /// Optional palette category path for grouping nodes in authoring tools.
        /// </summary>
        public string Category { get; }
    }
}