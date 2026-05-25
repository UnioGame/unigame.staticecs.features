using System;
using FFS.Libraries.StaticEcs;

namespace unigame.staticecs.features {
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
    public struct EffectRosterEntry : IMultiComponent {
        public EffectId Id;
        public int Stacks;
        public float TimeLeft;
    }
}
