namespace UniGame.StaticEcs.Features
{
    using System;
    using FFS.Libraries.StaticEcs;

    /// <summary>Stores one modifier applied to a characteristic.</summary>
    [Serializable]
    public struct CharacteristicModifierComponent<TCharacteristic> : IMultiComponent
        where TCharacteristic : struct, ICharacteristicType
    {
        /// <summary>The entity that owns the modifier.</summary>
        public EntityGIDCompact Source;

        /// <summary>The modifier operand.</summary>
        public float Value;

        /// <summary>The operation used to apply the operand.</summary>
        public CharacteristicModifierOp Op;
    }
}
