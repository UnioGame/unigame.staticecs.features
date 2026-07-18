using System.Collections.Generic;
using FFS.Libraries.StaticEcs;
using Pathfinding;
using Pathfinding.Graphs.Grid;
using UnityEngine;

namespace unigame.staticecs.features {
    /// <summary>Creates ECS-owned A* grid graphs and synchronizes ECS obstacle entities.</summary>
    public class AstarGraphSystem<TWorld> : ISystem
        where TWorld : struct, IWorldType {
        private const float BoundsChangeSqrThreshold = 0.000001f;

        private readonly HashSet<AstarPath> _backendsToFlush = new();

        /// <inheritdoc/>
        public void Update() {
            InitializeGraphs();

            if (RequiresPhysicsSync()) {
                Physics.SyncTransforms();
            }

            UpdateObstacles();
            FlushGraphUpdates();
        }

        /// <inheritdoc/>
        public void Destroy() {
            foreach (var entity in World<TWorld>
                         .Query<All<AstarPathComponent, AstarGridGraphRuntimeComponent>>()
                         .Entities()) {
                ref var runtime = ref entity.Mut<AstarGridGraphRuntimeComponent>();
                var backend = entity.Read<AstarPathComponent>().Backend;
                if (backend != null && runtime.Graph != null) {
                    backend.data.RemoveGraph(runtime.Graph);
                }

                runtime.Graph = null;
            }

            _backendsToFlush.Clear();
        }

        private static void InitializeGraphs() {
            foreach (var entity in World<TWorld>
                         .Query<All<AstarPathComponent, AstarGridGraphConfigComponent>,
                             None<AstarGraphInitializedTag, AstarGraphInitializationFailedTag>>()
                         .Entities()) {
                var backend = entity.Read<AstarPathComponent>().Backend;
                if (backend == null || backend.data == null) {
                    continue;
                }

                if (AstarPath.active == null) {
                    continue;
                }

                if (backend != AstarPath.active) {
                    entity.Set<AstarGraphInitializationFailedTag>();
                    Debug.LogError(
                        $"[Static ECS A*] Graph backend '{backend.name}' is not AstarPath.active " +
                        $"('{AstarPath.active.name}'). A* supports one active backend per scene.");
                    continue;
                }

                ref readonly var config = ref entity.Read<AstarGridGraphConfigComponent>();
                var graph = backend.data.AddGraph(typeof(GridGraph)) as GridGraph;
                if (graph == null) {
                    continue;
                }

                graph.name = "StaticEcsGrid";
                graph.center = config.Center;
                graph.rotation = config.Rotation;
                graph.SetDimensions(
                    Mathf.Max(1, config.Width),
                    Mathf.Max(1, config.Depth),
                    Mathf.Max(0.1f, config.NodeSize));
                graph.collision.collisionCheck = true;
                graph.collision.heightCheck = false;
                graph.collision.type = ColliderType.Capsule;
                graph.collision.mask = config.ObstacleMask;
                graph.collision.diameter = Mathf.Max(0.1f, config.AgentDiameter) / graph.nodeSize;
                graph.collision.height = Mathf.Max(0f, config.AgentHeight);

                backend.Scan(graph);
                var nodeCount = 0;
                var walkableNodeCount = 0;
                graph.GetNodes(node => {
                    nodeCount++;
                    if (node.Walkable) {
                        walkableNodeCount++;
                    }
                });

                entity.Set(new AstarGridGraphRuntimeComponent {
                    Graph = graph,
                    NodeCount = nodeCount,
                    WalkableNodeCount = walkableNodeCount,
                });

                var registered = System.Array.IndexOf(backend.data.graphs, graph) >= 0;
                if (!registered || !graph.isScanned || nodeCount == 0 || walkableNodeCount == 0) {
                    entity.Set<AstarGraphInitializationFailedTag>();
                    Debug.LogError(
                        $"[Static ECS A*] Grid scan is unusable: registered={registered}, " +
                        $"scanned={graph.isScanned}, nodes={nodeCount}, " +
                        $"walkable={walkableNodeCount}, center={config.Center}, " +
                        $"dimensions={config.Width}x{config.Depth}, nodeSize={config.NodeSize}.");
                    continue;
                }

                entity.Set<AstarGraphInitializedTag>();
            }
        }

        private static bool RequiresPhysicsSync() {
            foreach (var entity in World<TWorld>.Query<All<AstarObstacleComponent>>().Entities()) {
                ref readonly var obstacle = ref entity.Read<AstarObstacleComponent>();
                var collider = obstacle.Collider;
                if (collider == null || !obstacle.HasSnapshot) {
                    continue;
                }

                if (collider.transform.localToWorldMatrix != obstacle.LastLocalToWorld) {
                    return true;
                }
            }

            return false;
        }

        private void UpdateObstacles() {
            foreach (var entity in World<TWorld>.Query<All<AstarObstacleComponent>>().Entities()) {
                ref var obstacle = ref entity.Mut<AstarObstacleComponent>();
                if (!obstacle.GraphEntity.TryUnpack<TWorld>(out _)
                    && obstacle.GraphProvider != null) {
                    obstacle.GraphEntity = obstacle.GraphProvider.EntityGid;
                }

                if (!obstacle.GraphEntity.TryUnpack<TWorld>(out var graphEntity)
                    || !graphEntity.Has<AstarGraphInitializedTag>()) {
                    continue;
                }

                var collider = obstacle.Collider;
                var isActive = collider != null
                               && collider.enabled
                               && collider.gameObject.activeInHierarchy;
                var currentMatrix = collider != null
                    ? collider.transform.localToWorldMatrix
                    : obstacle.LastLocalToWorld;
                var currentBounds = isActive ? collider.bounds : obstacle.LastBounds;

                if (!obstacle.HasSnapshot) {
                    obstacle.HasSnapshot = true;
                    obstacle.WasActive = isActive;
                    obstacle.LastLocalToWorld = currentMatrix;
                    obstacle.LastBounds = currentBounds;
                    if (isActive) {
                        QueueUpdate(obstacle.GraphEntity, currentBounds);
                    }
                    continue;
                }

                var stateChanged = isActive != obstacle.WasActive;
                var transformChanged = currentMatrix != obstacle.LastLocalToWorld;
                var boundsChanged = isActive && BoundsChanged(obstacle.LastBounds, currentBounds);
                if (!stateChanged && !transformChanged && !boundsChanged) {
                    continue;
                }

                if (obstacle.WasActive) {
                    QueueUpdate(obstacle.GraphEntity, obstacle.LastBounds);
                }
                if (isActive) {
                    QueueUpdate(obstacle.GraphEntity, currentBounds);
                }

                obstacle.WasActive = isActive;
                obstacle.LastLocalToWorld = currentMatrix;
                obstacle.LastBounds = currentBounds;
            }
        }

        private void QueueUpdate(EntityGID graphEntity, Bounds bounds) {
            var backend = AstarGraphUpdateUtility.UpdateBounds<TWorld>(graphEntity, bounds, flushOverride: false);
            if (backend == null || !graphEntity.TryUnpack<TWorld>(out var entity)
                || !entity.Has<AstarGridGraphConfigComponent>()
                || !entity.Read<AstarGridGraphConfigComponent>().FlushGraphUpdates) {
                return;
            }

            _backendsToFlush.Add(backend);
        }

        private void FlushGraphUpdates() {
            foreach (var backend in _backendsToFlush) {
                if (backend != null) {
                    backend.FlushGraphUpdates();
                }
            }

            _backendsToFlush.Clear();
        }

        private static bool BoundsChanged(Bounds previous, Bounds current) {
            return (previous.center - current.center).sqrMagnitude > BoundsChangeSqrThreshold
                   || (previous.extents - current.extents).sqrMagnitude > BoundsChangeSqrThreshold;
        }
    }
}
