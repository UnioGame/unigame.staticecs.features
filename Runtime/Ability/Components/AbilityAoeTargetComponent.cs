namespace UniGame.StaticEcs.Features
{
    using FFS.Libraries.StaticEcs;

    /// <summary>Stores one target selected by an ability area query.</summary>
    public struct AbilityAoeTargetComponent : IMultiComponent
    {
        /// <summary>The selected target entity.</summary>
        public EntityGID Target;
    }
}
