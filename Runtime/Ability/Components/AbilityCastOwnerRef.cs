using FFS.Libraries.StaticEcs;

namespace UniGame.StaticEcs.Features {
    /// <summary>
    /// Root caster for a cast-entity. For root casts equals
    /// <see cref="AbilityCastRuntimeComponent.Caster"/>; for sub-ability child casts inherits
    /// the parent's owner so cooldown / cost lookups by Owner stay stable along the chain.
    /// </summary>
    public struct AbilityCastOwnerRef : IComponent {
        public EntityGID Owner;
    }
}
