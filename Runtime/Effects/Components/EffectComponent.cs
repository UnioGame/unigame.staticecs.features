namespace UniGame.StaticEcs.Features
{
    using System;
    using FFS.Libraries.StaticEcs;

    /// <summary>
    /// Per-target lifetime state for a single effect type. Owned by <c>EffectTickSystem</c>
    /// which decrements timers, fires periodic ticks, and removes the component on expiry.
    /// Per-effect gameplay payload lives in a sibling component (e.g. <c>HealOverTimeComponent</c>).
    ///
    /// Single-source contract: an effect entry is always tied to one source entity. Re-applying
    /// from a different source overwrites <see cref="Source"/> — this is intentional for the
    /// current iteration. Multi-source semantics live in dedicated structures
    /// (e.g. <c>StunSourceComponent</c> + <c>StunOperations</c>).
    /// </summary>
    [Serializable]
    public struct EffectComponent<TEffect> : IComponent
        where TEffect : struct, IEffectType
    {
        public EntityGID Source;
        public float TimeLeft;
        public float PeriodLeft;
        public float Period;
        public int Stacks;

        /// <summary>Activations left; used by <see cref="EffectDurationMode.Charges"/> effects.</summary>
        public int ChargesLeft;

        /// <summary>Strength given at the first application, for example damage per activation.</summary>
        public float Magnitude;

        /// <summary>Name of the source captured at the last application; history only, may outlive the source.</summary>
        public string SourceName;
    }
}
