using System;
using System.Reflection;

namespace UniGame.StaticEcs.Features {
    public static class CharacteristicFlagOf<T> where T : struct {
        public static readonly CharacteristicFlag Value = Resolve();

        private static CharacteristicFlag Resolve() {
            var attr = typeof(T).GetCustomAttribute<CharacteristicFlagAttribute>();
            if (attr == null) {
                throw new InvalidOperationException(
                    $"{typeof(T).Name} must declare [CharacteristicFlag(...)] to participate in modifier source-cleanup.");
            }

            var raw = (ulong)attr.Flag;
            if (raw == 0 || (raw & (raw - 1)) != 0) {
                throw new InvalidOperationException(
                    $"{typeof(T).Name} flag {attr.Flag} must be a single power of two.");
            }

            return attr.Flag;
        }
    }
}
