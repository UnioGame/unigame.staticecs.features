using System;
using FFS.Libraries.StaticEcs;

namespace UniGame.StaticEcs.Features {
    /// <summary>
    /// Per-target modifier payload consumed by <see cref="ModificationEffectHandler{TWorld, TStat}"/>.
    /// Re-application overwrites the value; the underlying modifier is keyed by source so
    /// repeated apply from the same source replaces the previous modifier in place.
    /// </summary>
    [Serializable]
    public struct ModificationEffectData<TStat> : IComponent
        where TStat : struct, ICharacteristicType {
        public CharacteristicModifierOp Op;
        public float Value;
    }
}
