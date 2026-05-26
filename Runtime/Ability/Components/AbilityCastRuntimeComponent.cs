using FFS.Libraries.StaticEcs;

namespace unigame.staticecs.features {
    /// <summary>
    /// Per cast-entity runtime state. Lives on a dedicated cast-entity created by
    /// <c>AbilityCastSystem</c>. Replaces the legacy <c>AbilityCastComponent</c> which lived on
    /// the caster — this version supports concurrent casts (foreground + channels + sub-ability
    /// branches) by giving every active cast its own entity.
    ///
    /// <see cref="RootEntered"/> is false until the progression system descends into the root
    /// step; once set, an empty stack means the cast has completed.
    /// </summary>
    public struct AbilityCastRuntimeComponent : IComponent {
        public AbilityId AbilityId;
        public EntityGID Caster;
        public EntityGID PrimaryTarget;
        public bool RootEntered;
    }
}
