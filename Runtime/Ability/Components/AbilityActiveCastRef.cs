using FFS.Libraries.StaticEcs;

namespace UniGame.StaticEcs.Features {
    /// <summary>
    /// Pointer from a caster to its current foreground cast-entity. Absent when the caster
    /// is idle. Channels and parallel branches do not occupy this slot — they live in
    /// <see cref="AbilityChannelCastRef"/> instead, so the caster can hold one foreground cast
    /// concurrent with N channels.
    /// </summary>
    public struct AbilityActiveCastRef : IComponent {
        public EntityGID Cast;
    }
}
