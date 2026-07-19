using FFS.Libraries.StaticEcs;

namespace UniGame.StaticEcs.Features {
    /// <summary>
    /// One open composite frame on the cast-entity progression stack. Stored as a multi-component
    /// so the stack lives entirely inside ECS storage — no managed coordinator object.
    /// </summary>
    /// <remarks>
    /// Semantics of <see cref="Cursor"/>:
    /// <list type="bullet">
    ///   <item>Sequence — index of the currently-running child (0-based). Advanced by the
    ///   progression system on child completion. When it reaches <see cref="ChildrenTotal"/> the
    ///   frame pops.</item>
    ///   <item>Parallel — child branch count is tracked through <see cref="SuccessCount"/> and
    ///   <see cref="FailedCount"/>.</item>
    ///   <item>Repeat — completed iteration index.</item>
    /// </list>
    /// </remarks>
    public struct AbilityStackFrame : IMultiComponent {
        public IAbilityStepConfig Composite;
        public AbilityStepKind Kind;
        public int Cursor;
        public int ChildrenTotal;
        public int SuccessCount;
        public int FailedCount;
    }
}
