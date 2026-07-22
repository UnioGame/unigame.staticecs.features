namespace UniGame.StaticEcs.Features
{
    using FFS.Libraries.StaticEcs;

    /// <summary>
    /// Root caster for a cast-entity. For root casts equals
    /// <see cref="AbilityCastComponent.Caster"/>; for sub-ability child casts inherits
    /// the parent's owner so cooldown / cost lookups by Owner stay stable along the chain.
    /// </summary>
    public struct AbilityCastOwnerComponent : IComponent
    {
        /// <summary>The root entity that owns the cast.</summary>
        public EntityGID Owner;
    }
}
