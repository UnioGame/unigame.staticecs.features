using FFS.Libraries.StaticEcs;
using Pathfinding;
using UnityEngine;

namespace unigame.staticecs.features {
    internal static class AstarGraphUpdateUtility {
        public static AstarPath UpdateBounds<TWorld>(
            EntityGID graphGid,
            Bounds bounds,
            bool? flushOverride)
            where TWorld : struct, IWorldType {
            if (!graphGid.TryUnpack<TWorld>(out var graphEntity)
                || !graphEntity.Has<AstarPathComponent>()
                || !graphEntity.Has<AstarGridGraphRuntimeComponent>()) {
                return null;
            }

            var backend = graphEntity.Read<AstarPathComponent>().Backend;
            var graph = graphEntity.Read<AstarGridGraphRuntimeComponent>().Graph;
            if (backend == null || graph == null) {
                return null;
            }

            backend.UpdateGraphs(new GraphUpdateObject(bounds) { updatePhysics = true });

            var shouldFlush = flushOverride
                              ?? (graphEntity.Has<AstarGridGraphConfigComponent>()
                                  && graphEntity.Read<AstarGridGraphConfigComponent>().FlushGraphUpdates);
            if (shouldFlush) {
                backend.FlushGraphUpdates();
            }

            return backend;
        }
    }
}
