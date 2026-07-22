namespace UniGame.StaticEcs.Features
{
    using FFS.Libraries.StaticEcs;

    /// <summary>
    /// Per-caster list of currently running channel cast-entities.
    /// </summary>
    public struct AbilityChannelCastComponent : IMultiComponent
    {
        /// <summary>The running channel cast entity.</summary>
        public EntityGID Cast;
    }
}
