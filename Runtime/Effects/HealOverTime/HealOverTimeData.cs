using System;
using FFS.Libraries.StaticEcs;

namespace UniGame.StaticEcs.Features {
    /// <summary>
    /// Per-target heal payload consumed by <see cref="HealOverTimeHandler{TWorld}"/> on every
    /// periodic tick. Re-applying the effect overwrites the value with the latest amount.
    /// </summary>
    [Serializable]
    public struct HealOverTimeData : IComponent {
        public float HealPerTick;
    }
}
