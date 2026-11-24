namespace UniGame.StaticEcs.Features
{
    using System;
    using FFS.Libraries.StaticEcs;

    /// <summary>Stores one entity contributing to the target's active immobilize state.</summary>
    [Serializable]
    [CharacteristicFlag(CharacteristicFlag.Immobilize)]
    public struct ImmobilizeSourceComponent : IMultiComponent
    {
        /// <summary>The entity that contributes the immobilize state.</summary>
        public EntityGID Source;
    }
}
