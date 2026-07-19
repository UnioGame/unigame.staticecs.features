namespace UniGame.StaticEcs.Features {
    /// <summary>
    /// Marker for heal-over-time effects. Pairs with <see cref="HealOverTimeData"/> on the target
    /// and a <see cref="HealOverTimeHandler{TWorld}"/> handler resource.
    /// </summary>
    [EffectFlag(EffectFlag.HealOverTime)]
    public struct HealOverTimeEffect : IEffectType { }
}
