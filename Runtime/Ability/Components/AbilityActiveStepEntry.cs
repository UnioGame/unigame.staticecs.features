using FFS.Libraries.StaticEcs;

namespace UniGame.StaticEcs.Features {
    /// <summary>
    /// Observability entry on a cast-entity describing one currently active step. Most casts
    /// hold a single entry; Parallel branches may hold several across branch cast-entities.
    /// </summary>
    public struct AbilityActiveStepEntry : IMultiComponent {
        public string NodeGuid;
        public AbilityStepKind Kind;
        public float StartedAt;
    }
}
