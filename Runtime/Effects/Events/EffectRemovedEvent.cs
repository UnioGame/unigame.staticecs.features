using FFS.Libraries.StaticEcs;

namespace unigame.staticecs.features {
    /// <summary>
    /// Raised when an effect leaves the target — either by manual removal through
    /// <c>EffectOperations.Remove</c>, by expiry inside <c>EffectTickSystem</c>, or by
    /// source-dead cleanup. <see cref="Expired"/> distinguishes natural lifetime end from
    /// explicit removal.
    /// </summary>
    public struct EffectRemovedEvent<TEffect> : IEvent
        where TEffect : struct, IEffectType {
        public EntityGID Source;
        public EntityGID Target;
        public int Stacks;
        public bool Expired;
    }
}
