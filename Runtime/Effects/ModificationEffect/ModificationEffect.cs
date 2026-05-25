using FFS.Libraries.StaticEcs;

namespace unigame.staticecs.features {
    /// <summary>
    /// Marker for time-bounded characteristic modifications. Pairs with
    /// <see cref="ModificationEffectData{TStat}"/> on the target and a
    /// <see cref="ModificationEffectHandler{TWorld, TStat}"/> handler resource.
    /// </summary>
    [EffectFlag(EffectFlag.Modification)]
    public struct ModificationEffect<TStat> : IEffectType
        where TStat : struct, ICharacteristicType { }
}
