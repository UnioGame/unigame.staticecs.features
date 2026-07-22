namespace UniGame.StaticEcs.Features
{
    using System;
    using FFS.Libraries.StaticEcs;

    /// <summary>
    /// Per-target heal payload consumed by <see cref="HealOverTimeHandler{TWorld}"/> on every
    /// periodic tick. Re-applying the effect overwrites the value with the latest amount.
    /// </summary>
    [Serializable]
    public struct HealOverTimeComponent : IComponent
    {
        /// <summary>The healing applied by each periodic tick.</summary>
        public float HealPerTick;
    }
}
