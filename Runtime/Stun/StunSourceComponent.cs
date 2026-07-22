namespace UniGame.StaticEcs.Features
{
    using System;
    using FFS.Libraries.StaticEcs;

    /// <summary>Stores one entity contributing to the target's active stun state.</summary>
    [Serializable]
    [CharacteristicFlag(CharacteristicFlag.Stun)]
    public struct StunSourceComponent : IMultiComponent
    {
        /// <summary>The entity that contributes the stun state.</summary>
        public EntityGIDCompact Source;
    }
}
