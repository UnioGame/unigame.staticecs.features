using System;
using System.Reflection;

namespace unigame.staticecs.features {
    /// <summary>
    /// Bitmask identifier for effect types. Each <see cref="IEffectType"/> declares its own
    /// single-bit flag through <see cref="EffectFlagAttribute"/>; group masks (e.g.
    /// <see cref="CrowdControl"/>) are unions used by <c>EffectOperations.RemoveByMask</c>.
    ///
    /// Generic <see cref="ModificationEffect{TStat}"/> shares one flag for all TStat closures —
    /// source-destroy cleanup snaps every characteristic modification on every target in a
    /// single bit pass. Granular removal still goes through typed
    /// <c>EffectOperations.Remove&lt;TWorld, ModificationEffect&lt;TStat&gt;&gt;</c>.
    /// </summary>
    [Flags]
    public enum EffectFlag : ulong {
        None         = 0,
        HealOverTime = 1ul << 0,
        Stun         = 1ul << 1,
        Modification = 1ul << 2,

        /// <summary>Convenience group: every CC-style effect (currently only <see cref="Stun"/>).</summary>
        CrowdControl = Stun,

        /// <summary>
        /// Reserved single-bit slots for project-side or test-only effects.
        /// Bit 63 is intentionally left empty: enum drawers in Unity / Odin convert each value
        /// to <see cref="long"/> and the sign bit overflows <see cref="System.Int64"/>.
        /// </summary>
        Reserved0 = 1ul << 59,
        Reserved1 = 1ul << 60,
        Reserved2 = 1ul << 61,
        Reserved3 = 1ul << 62
    }

    [AttributeUsage(AttributeTargets.Struct, AllowMultiple = false, Inherited = false)]
    public sealed class EffectFlagAttribute : Attribute {
        public EffectFlag Flag { get; }

        public EffectFlagAttribute(EffectFlag flag) {
            Flag = flag;
        }
    }

    /// <summary>
    /// Per-(closed)-type cache of <see cref="EffectFlag"/>. Resolved once per process via
    /// reflection on the open-generic definition (so all <c>ModificationEffect&lt;TStat&gt;</c>
    /// closures share the attribute).
    /// </summary>
    public static class EffectFlagOf<T> where T : struct {
        public static readonly EffectFlag Value = Resolve();

        private static EffectFlag Resolve() {
            var attr = typeof(T).GetCustomAttribute<EffectFlagAttribute>();
            if (attr == null) {
                throw new InvalidOperationException(
                    $"{typeof(T).Name} must declare [EffectFlag(...)] to participate in effect source-cleanup.");
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
