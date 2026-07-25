namespace UniGame.StaticEcs.Features
{
    using FFS.Libraries.StaticEcs;
    using UnityEngine;

    /// <summary>
    /// Spatial index over targetable entities. The current implementation is rebuilt by
    /// <c>TargetIndexRebuildSystem</c>; abilities and AoE queries call <see cref="FillSphere"/>
    /// at resolution time. The index owns no entity state — it is a transient spatial accelerator
    /// keyed by <see cref="EntityGID"/>.
    /// </summary>
    public interface ITargetIndex<TWorld> : IResource
        where TWorld : struct, IWorldType
    {
        int Count { get; }

        void Rebuild();

        /// <summary>Fills the output with the nearest targets ordered by distance and entity id.</summary>
        int FillNearestSphere(
            Vector3 center,
            float radius,
            System.Span<EntityGID> output,
            EntityGID excluded = default);

        /// <summary>Fills the output with an unordered bounded radius query.</summary>
        int FillSphere(Vector3 center, float radius, System.Span<EntityGID> output);
    }
}
