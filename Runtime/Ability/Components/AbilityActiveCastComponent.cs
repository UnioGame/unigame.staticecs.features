namespace UniGame.StaticEcs.Features
{
    using FFS.Libraries.StaticEcs;

    /// <summary>
    /// Pointer from a caster to its current foreground cast-entity. Absent when the caster
    /// is idle. Channels and parallel branches do not occupy this slot — they live in
    /// <see cref="AbilityChannelCastComponent"/> instead, so the caster can hold one foreground cast
    /// concurrent with N channels.
    /// </summary>
    public struct AbilityActiveCastComponent : IComponent
    {
        /// <summary>The active foreground cast entity.</summary>
        public EntityGID Cast;
    }
}
