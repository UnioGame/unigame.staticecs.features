using FFS.Libraries.StaticEcs;

namespace UniGame.StaticEcs.Features {
    /// <summary>
    /// Reference from a sub-ability cast-entity to its parent. Absent on root casts. Used by
    /// <c>AbilityChildCleanupSystem</c> (PR #4) to cascade cancellation from parent to child
    /// when the parent is destroyed.
    /// </summary>
    public struct AbilityCastParentRef : IComponent {
        public EntityGID Parent;
    }
}
