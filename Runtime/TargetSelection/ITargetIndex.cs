using FFS.Libraries.StaticEcs;
using UnityEngine;

namespace unigame.staticecs.features {
    /// <summary>
    /// Spatial index over targetable entities. The current implementation is rebuilt by
    /// <c>TargetIndexRebuildSystem</c>; abilities and AoE queries call <see cref="FillSphere"/>
    /// at resolution time. The index owns no entity state — it is a transient spatial accelerator
    /// keyed by <see cref="EntityGID"/>.
    /// </summary>
    public interface ITargetIndex<TWorld> : IResource
        where TWorld : struct, IWorldType {
        int Count { get; }

        void Rebuild();

        int FillSphere(Vector3 center, float radius, System.Span<EntityGID> output);
    }
}
