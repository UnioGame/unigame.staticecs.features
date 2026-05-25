using FFS.Libraries.StaticEcs;

namespace unigame.staticecs.features {
    /// <summary>
    /// Raised by <c>EffectOperations.Apply</c> when a fresh <see cref="EffectComponent{TEffect}"/>
    /// is added to the target. Re-application of an already-active effect raises
    /// <see cref="EffectRefreshedEvent{TEffect}"/> instead.
    /// </summary>
    public struct EffectAppliedEvent<TEffect> : IEvent
        where TEffect : struct, IEffectType {
        public EntityGID Source;
        public EntityGID Target;
        public int Stacks;
        public float TimeLeft;
    }
}
