namespace UniGame.StaticEcs.Features {
    /// <summary>
    /// Auto-assigns a stable bit index (0–31) to <typeparamref name="TAction"/> on first access.
    /// The index is allocated once per process lifetime and is consistent for the lifetime of the
    /// AppDomain. The total number of distinct action types must not exceed 32.
    /// </summary>
    public static class ActionBit<TAction> where TAction : struct, IGameAction {
        // ReSharper disable once StaticMemberInGenericType
        private static readonly int IndexValue = ActionBitRegistry.Allocate();

        /// <summary>Zero-based bit index for this action type (0–31).</summary>
        public static int Index => IndexValue;

        /// <summary>Single-bit mask derived from <see cref="Index"/>.</summary>
        public static uint Mask => 1u << IndexValue;
    }
}
