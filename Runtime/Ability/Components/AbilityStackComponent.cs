namespace UniGame.StaticEcs.Features
{
    using FFS.Libraries.StaticEcs;

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
    public struct AbilityStackComponent : IMultiComponent
    {
        /// <summary>The composite step represented by this stack frame.</summary>
        public IAbilityStepConfig Composite;

        /// <summary>The composite step kind.</summary>
        public AbilityStepKind Kind;

        /// <summary>The current child or iteration cursor.</summary>
        public int Cursor;

        /// <summary>The total number of child steps.</summary>
        public int ChildrenTotal;

        /// <summary>The number of completed successful children.</summary>
        public int SuccessCount;

        /// <summary>The number of completed failed children.</summary>
        public int FailedCount;
    }
}
