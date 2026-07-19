using FFS.Libraries.StaticEcs;

namespace UniGame.StaticEcs.Features {
    /// <summary>
    /// Emitted whenever a leaf step becomes the active step on a cast-entity. Drives VFX /
    /// animation hooks via <see cref="NodeGuid"/> (designer-stable handle) or
    /// <see cref="Kind"/> (coarse category).
    /// </summary>
    public struct AbilityStepStartedEvent : IEvent {
        public EntityGID CastEntity;
        public AbilityId AbilityId;
        public string NodeGuid;
        public AbilityStepKind Kind;
    }
}
