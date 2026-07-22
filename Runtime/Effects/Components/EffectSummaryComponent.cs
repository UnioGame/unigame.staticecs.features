namespace UniGame.StaticEcs.Features
{
    using System;
    using FFS.Libraries.StaticEcs;

    /// <summary>
    /// Single roster row, agnostic of the concrete effect type. Updated by
    /// <c>EffectOperations</c> on apply, refresh, and removal — never on a per-tick basis.
    ///
    /// <para><see cref="TimeLeft"/> reflects the duration captured at the last lifecycle event;
    /// for live remaining time call <c>EffectOperations.GetTimeLeft&lt;TWorld, TEffect&gt;</c>
    /// or read the typed <see cref="EffectComponent{TEffect}"/> directly.</para>
    ///
    /// <para><see cref="Stacks"/> matches the typed component value at the same lifecycle event
    /// boundaries.</para>
    /// </summary>
    [Serializable]
    public struct EffectSummaryComponent : IMultiComponent
    {
        /// <summary>The effect identifier.</summary>
        public EffectId Id;

        /// <summary>The stack count captured at the latest lifecycle event.</summary>
        public int Stacks;

        /// <summary>The remaining duration captured at the latest lifecycle event.</summary>
        public float TimeLeft;
    }
}
