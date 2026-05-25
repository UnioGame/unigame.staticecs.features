namespace unigame.staticecs.features {
    /// <summary>
    /// Marker for time-bounded stuns. <see cref="StunEffectHandler{TWorld}"/> attaches a
    /// <see cref="StunSource"/> entry on apply (so the existing multi-source counter and
    /// <c>StunActiveTag</c> logic stays authoritative) and removes it on expire.
    /// </summary>
    [EffectFlag(EffectFlag.Stun)]
    public struct StunEffect : IEffectType { }
}
