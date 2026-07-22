namespace UniGame.StaticEcs.Features
{
    using FFS.Libraries.StaticEcs;

    /// <summary>
    /// Single ability slot equipped on a caster. Stored as a multi-component;
    /// <see cref="AbilityOperations.Equip{TWorld}"/> guarantees uniqueness by id.
    /// </summary>
    public struct AbilitySlotComponent : IMultiComponent
    {
        /// <summary>The equipped ability identifier.</summary>
        public AbilityId Id;
    }
}
