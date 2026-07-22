namespace UniGame.StaticEcs.Features
{
    using System;
    using FFS.Libraries.StaticEcs;

    /// <summary>
    /// Per-target modifier payload consumed by <see cref="ModificationEffectHandler{TWorld, TStat}"/>.
    /// Re-application overwrites the value; the underlying modifier is keyed by source so
    /// repeated apply from the same source replaces the previous modifier in place.
    /// </summary>
    [Serializable]
    public struct ModificationEffectComponent<TStat> : IComponent
        where TStat : struct, ICharacteristicType
    {
        /// <summary>The characteristic operation applied by the effect.</summary>
        public CharacteristicModifierOp Op;

        /// <summary>The modifier operand.</summary>
        public float Value;
    }
}
