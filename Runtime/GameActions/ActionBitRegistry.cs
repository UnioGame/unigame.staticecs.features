using System;
using System.Threading;

namespace unigame.staticecs.features {
    /// <summary>
    /// Thread-safe allocator that assigns a unique bit index (0–31) to each
    /// <see cref="IGameAction"/> type on first access.
    /// </summary>
    internal static class ActionBitRegistry {
        private static int _counter;

        internal static int Allocate() {
            var index = Interlocked.Increment(ref _counter) - 1;
            if (index >= 32) {
                throw new InvalidOperationException(
                    "ActionBit overflow: more than 32 distinct IGameAction types registered. " +
                    "Consider widening ActionMaskComponent.Bits to ulong.");
            }

            return index;
        }
    }
}
