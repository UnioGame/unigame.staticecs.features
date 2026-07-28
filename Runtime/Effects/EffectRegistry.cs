namespace UniGame.StaticEcs.Features
{
    using System;
    using FFS.Libraries.StaticEcs;

    public delegate void EffectSourceCleanup(EntityGID source, EntityGID target);

    public delegate void EffectUnconditionalRemove(EntityGID target);

    /// <summary>
    /// Per-world resource that maps <see cref="EffectFlag"/> bits to a removal callback. Each
    /// <c>EffectFeature&lt;TWorld, TEffect&gt;</c> registers a slot during initialization;
    /// <see cref="EffectTrackerComponent"/> consumes the registry on source destroy and
    /// <c>EffectOperations.RemoveByMask</c> uses it for grouped removal.
    /// </summary>
    public sealed class EffectRegistry : IResource
    {
        public const int MaxSlots = 64;

        private readonly EffectSourceCleanup[] _slots = new EffectSourceCleanup[MaxSlots];
        private readonly EffectUnconditionalRemove[] _removeSlots = new EffectUnconditionalRemove[
            MaxSlots
        ];
        private ulong _registeredMask;

        public ulong RegisteredMask => _registeredMask;

        public void Register(
            EffectFlag flag,
            EffectSourceCleanup cleanup,
            EffectUnconditionalRemove unconditional
        )
        {
            if (cleanup == null)
                throw new ArgumentNullException(nameof(cleanup));

            if (unconditional == null)
                throw new ArgumentNullException(nameof(unconditional));

            var raw = (ulong)flag;
            if (!IsSingleBit(raw))
                throw new ArgumentException("Flag must be a single power of two.", nameof(flag));

            var slot = Log2(raw);
            _slots[slot] = cleanup;
            _removeSlots[slot] = unconditional;
            _registeredMask |= raw;
        }

        public bool IsRegistered(EffectFlag flag)
        {
            var raw = (ulong)flag;
            if (!IsSingleBit(raw))
                return false;

            return _slots[Log2(raw)] != null;
        }

        public void Invoke(EffectFlag flag, EntityGID source, EntityGID target)
        {
            var raw = (ulong)flag;
            if (!IsSingleBit(raw))
                return;

            _slots[Log2(raw)]?.Invoke(source, target);
        }

        public void InvokeMask(EffectFlag mask, EntityGID source, EntityGID target)
        {
            var raw = (ulong)mask;
            while (raw != 0)
            {
                var bit = raw & (0UL - raw);
                _slots[Log2(bit)]?.Invoke(source, target);
                raw ^= bit;
            }
        }

        public void InvokeRemove(EffectFlag flag, EntityGID target)
        {
            var raw = (ulong)flag;
            if (!IsSingleBit(raw))
                return;

            _removeSlots[Log2(raw)]?.Invoke(target);
        }

        public void Reset()
        {
            Array.Clear(_slots, 0, _slots.Length);
            Array.Clear(_removeSlots, 0, _removeSlots.Length);
            _registeredMask = 0;
        }

        private static bool IsSingleBit(ulong value)
        {
            return value != 0 && (value & (value - 1)) == 0;
        }

        private static int Log2(ulong singleBit)
        {
            var n = 0;
            if ((singleBit & 0xFFFFFFFF00000000UL) != 0)
            {
                singleBit >>= 32;
                n += 32;
            }
            if ((singleBit & 0x00000000FFFF0000UL) != 0)
            {
                singleBit >>= 16;
                n += 16;
            }
            if ((singleBit & 0x000000000000FF00UL) != 0)
            {
                singleBit >>= 8;
                n += 8;
            }
            if ((singleBit & 0x00000000000000F0UL) != 0)
            {
                singleBit >>= 4;
                n += 4;
            }
            if ((singleBit & 0x000000000000000CUL) != 0)
            {
                singleBit >>= 2;
                n += 2;
            }
            if ((singleBit & 0x0000000000000002UL) != 0)
                n += 1;
            return n;
        }
    }
}
