using FFS.Libraries.StaticEcs;

namespace UniGame.StaticEcs.Features {
    /// <summary>
    /// Per-entity bitmask that gates <see cref="GameActionOperations.Raise{TWorld,TAction}"/> calls.
    /// Each bit corresponds to one <see cref="IGameAction"/> type via <see cref="ActionBit{TAction}"/>.
    /// <para>
    /// <c>uint.MaxValue</c> — all 32 slots enabled (default after adding the component).<br/>
    /// <c>0</c> — all actions disabled (e.g. fully stunned entity).<br/>
    /// Entities without this component are treated as fully enabled.
    /// </para>
    /// </summary>
    public struct ActionMaskComponent : IComponent {
        /// <summary>Bitmask of currently enabled action slots.</summary>
        public uint Bits;

        /// <summary>Creates a mask with all 32 action slots enabled.</summary>
        public static ActionMaskComponent AllEnabled => new() { Bits = uint.MaxValue };

        /// <summary>Creates a mask with all action slots disabled.</summary>
        public static ActionMaskComponent AllDisabled => new() { Bits = 0u };
    }
}
