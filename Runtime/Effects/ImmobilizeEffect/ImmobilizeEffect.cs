namespace UniGame.StaticEcs.Features
{
    /// <summary>
    /// Marker for time-bounded immobilizes. <see cref="ImmobilizeEffectHandler{TWorld}"/> attaches an
    /// <see cref="ImmobilizeSourceComponent"/> entry on apply (so the multi-source counter and
    /// <c>ImmobilizeActiveTag</c> stay authoritative) and removes it on expire. The package only keeps the
    /// state; the movement feature of the project decides what an immobilized entity cannot do.
    /// </summary>
    [EffectFlag(EffectFlag.Immobilize)]
    public struct ImmobilizeEffect : IEffectType { }
}
