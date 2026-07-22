namespace UniGame.StaticEcs.Features
{
    using System;
    using FFS.Libraries.StaticEcs;

    /// <summary>
    /// Back-reference stored on the effect <em>source</em> entity. One entry per affected target;
    /// <see cref="Mask"/> aggregates every effect flag this source has applied to that target,
    /// so a single bit-pass during source-destroy reaches all dependants.
    /// </summary>
    [Serializable]
    public struct EffectTargetComponent : IMultiComponent
    {
        /// <summary>The affected target entity.</summary>
        public EntityGIDCompact Target;

        /// <summary>The effect types applied to the target by this source.</summary>
        public EffectFlag Mask;
    }
}
