using System;
using FFS.Libraries.StaticEcs;

namespace UniGame.StaticEcs.Features {
    /// <summary>
    /// Back-reference stored on the effect <em>source</em> entity. One entry per affected target;
    /// <see cref="Mask"/> aggregates every effect flag this source has applied to that target,
    /// so a single bit-pass during source-destroy reaches all dependants.
    /// </summary>
    [Serializable]
    public struct EffectBackRef : IMultiComponent {
        public EntityGIDCompact Target;
        public EffectFlag Mask;
    }
}
