using FFS.Libraries.StaticEcs;

namespace UniGame.StaticEcs.Features {
    /// <summary>
    /// Single ability slot equipped on a caster. Stored as a multi-component;
    /// <see cref="AbilityOperations.Equip{TWorld}"/> guarantees uniqueness by id.
    /// </summary>
    public struct AbilityRosterEntry : IMultiComponent {
        public AbilityId Id;
    }
}
