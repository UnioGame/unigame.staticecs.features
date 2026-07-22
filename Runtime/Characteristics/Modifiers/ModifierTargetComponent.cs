namespace UniGame.StaticEcs.Features
{
    using System;
    using FFS.Libraries.StaticEcs;

    /// <summary>Tracks a target affected by modifiers from the current source entity.</summary>
    [Serializable]
    public struct ModifierTargetComponent : IMultiComponent
    {
        /// <summary>The affected target entity.</summary>
        public EntityGIDCompact Target;

        /// <summary>The characteristics affected on the target.</summary>
        public CharacteristicFlag StatMask;
    }
}
