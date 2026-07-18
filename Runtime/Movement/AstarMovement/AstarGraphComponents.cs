using System;
using FFS.Libraries.StaticEcs;
using FFS.Libraries.StaticEcs.Unity;
using Pathfinding;
using Pathfinding.Graphs.Grid;
using UnityEngine;

namespace unigame.staticecs.features {
    /// <summary>Stores the A* Pathfinding Project backend associated with a graph entity.</summary>
    [Serializable]
    public struct AstarPathComponent : IComponent {
        /// <summary>The Unity A* backend owned by the graph entity.</summary>
        public AstarPath Backend;
    }

    /// <summary>Defines the runtime grid graph created for an A* graph entity.</summary>
    [Serializable]
    public struct AstarGridGraphConfigComponent : IComponent {
        /// <summary>World-space center of the grid.</summary>
        public Vector3 Center;
        /// <summary>Euler rotation of the grid.</summary>
        public Vector3 Rotation;
        /// <summary>Number of nodes along the X axis.</summary>
        public int Width;
        /// <summary>Number of nodes along the Z axis.</summary>
        public int Depth;
        /// <summary>World-space size of one node.</summary>
        public float NodeSize;
        /// <summary>Unity layer mask used by collision sampling.</summary>
        public int ObstacleMask;
        /// <summary>World-space diameter of the collision capsule.</summary>
        public float AgentDiameter;
        /// <summary>World-space height of the collision capsule.</summary>
        public float AgentHeight;
        /// <summary>Whether queued dynamic obstacle updates are flushed in the same ECS tick.</summary>
        public bool FlushGraphUpdates;
    }

    /// <summary>Stores the grid graph created and owned by the ECS graph system.</summary>
    [Serializable]
    public struct AstarGridGraphRuntimeComponent : IComponent {
        /// <summary>The graph instance owned by this ECS entity.</summary>
        public GridGraph Graph;
        /// <summary>The number of nodes produced by the latest full scan.</summary>
        public int NodeCount;
        /// <summary>The number of walkable nodes produced by the latest full scan.</summary>
        public int WalkableNodeCount;
    }

    /// <summary>Marks an A* graph entity whose runtime graph has been created and scanned.</summary>
    public struct AstarGraphInitializedTag : ITag { }

    /// <summary>Marks an A* graph entity whose scan produced no usable walkable nodes.</summary>
    public struct AstarGraphInitializationFailedTag : ITag { }

    /// <summary>Tracks a Unity collider that updates an ECS-owned A* graph.</summary>
    [Serializable]
    public struct AstarObstacleComponent : IComponent {
        /// <summary>The authoring provider used to resolve the graph entity after deferred creation.</summary>
        public AbstractStaticEcsEntityProvider GraphProvider;
        /// <summary>The graph entity resolved from the configured entity provider.</summary>
        public EntityGID GraphEntity;
        /// <summary>The collider sampled by A* collision checks.</summary>
        public Collider Collider;
        /// <summary>The last bounds applied to the graph.</summary>
        public Bounds LastBounds;
        /// <summary>The last observed local-to-world transform.</summary>
        public Matrix4x4 LastLocalToWorld;
        /// <summary>Whether a previous obstacle state has been captured.</summary>
        public bool HasSnapshot;
        /// <summary>Whether the collider participated in the previous graph state.</summary>
        public bool WasActive;

        /// <summary>Clears the obstacle's previous footprint when its ECS entity is deleted.</summary>
        public void OnDelete<TWorld>(World<TWorld>.Entity self, HookReason reason)
            where TWorld : struct, IWorldType {
            if (reason == HookReason.WorldDestroy || !HasSnapshot || !WasActive) {
                return;
            }

            if (Collider != null && Collider.enabled) {
                Collider.enabled = false;
                Physics.SyncTransforms();
            }

            var graphEntity = GraphEntity;
            if (!graphEntity.TryUnpack<TWorld>(out _) && GraphProvider != null) {
                graphEntity = GraphProvider.EntityGid;
            }

            AstarGraphUpdateUtility.UpdateBounds<TWorld>(graphEntity, LastBounds, flushOverride: null);
        }
    }
}
