namespace UniGame.StaticEcs.Features
{
    using FFS.Libraries.StaticEcs;

    /// <summary>
    /// Observability entry on a cast-entity describing one currently active step. Most casts
    /// hold a single entry; Parallel branches may hold several across branch cast-entities.
    /// </summary>
    public struct AbilityActiveStepComponent : IMultiComponent
    {
        /// <summary>The stable graph-node identifier.</summary>
        public string NodeGuid;

        /// <summary>The kind of the active step.</summary>
        public AbilityStepKind Kind;

        /// <summary>The world time at which the step started.</summary>
        public float StartedAt;
    }
}
