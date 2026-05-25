using FFS.Libraries.StaticEcs;

namespace unigame.staticecs.features {
    /// <summary>
    /// Per-caster state of the active cast. Idle state is represented by absence of this component;
    /// only one ability may be active at a time per caster. <see cref="Target"/> is the caster's
    /// chosen primary target — <c>default</c> means "no target", handlers should call
    /// <see cref="EntityGID.TryUnpack"/> before dereferencing it.
    /// </summary>
    public struct AbilityCastComponent : IComponent {
        public AbilityId AbilityId;
        public AbilityPhase Phase;
        public float TimeLeft;
        public EntityGID Target;
    }
}
