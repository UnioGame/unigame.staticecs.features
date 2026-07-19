using FFS.Libraries.StaticEcs;

namespace UniGame.StaticEcs.Features {
    /// <summary>
    /// Marks an entity as a valid spell/ability target. Picked up by
    /// <see cref="ITargetIndex{TWorld}"/> implementations on rebuild.
    /// </summary>
    public struct TargetableTag : ITag { }
}
