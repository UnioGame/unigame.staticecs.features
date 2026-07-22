namespace UniGame.StaticEcs.Features
{
    /// <summary>
    /// Marker for time-bounded characteristic modifications. Pairs with
    /// <see cref="ModificationEffectComponent{TStat}"/> on the target and a
    /// <see cref="ModificationEffectHandler{TWorld, TStat}"/> handler resource.
    /// </summary>
    [EffectFlag(EffectFlag.Modification)]
    public struct ModificationEffect<TStat> : IEffectType
        where TStat : struct, ICharacteristicType { }
}
