using FFS.Libraries.StaticEcs;
using UnityEngine;

namespace UniGame.StaticEcs.Features
{
    using Unity;

    /// <summary>Converts an A* graph host into backend and grid configuration components.</summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(AstarPath))]
    public class AstarGridGraphConverter<TWorld> : EcsMonoConverter<TWorld>
        where TWorld : struct, IWorldType
    {
        [SerializeField]
        private Vector3 _center = Vector3.zero;

        [SerializeField]
        private Vector3 _rotation = Vector3.zero;

        [SerializeField, Min(1)]
        private int _width = 40;

        [SerializeField, Min(1)]
        private int _depth = 40;

        [SerializeField, Min(0.1f)]
        private float _nodeSize = 0.5f;

        [SerializeField]
        private LayerMask _obstacleMask = 1 << 6;

        [SerializeField, Min(0.1f)]
        private float _agentDiameter = 1.4f;

        [SerializeField, Min(0f)]
        private float _agentHeight = 1f;

        [SerializeField]
        private bool _flushGraphUpdates = true;

        /// <inheritdoc/>
        public override void Apply(World<TWorld>.Entity entity, GameObject host)
        {
            AstarGridGraphConverterUtility.Apply(
                entity,
                host,
                new AstarGridGraphConverterSettings(
                    _center,
                    _rotation,
                    _width,
                    _depth,
                    _nodeSize,
                    _obstacleMask,
                    _agentDiameter,
                    _agentHeight,
                    _flushGraphUpdates));
        }
    }

    internal static class AstarGridGraphConverterUtility
    {
        public static void Apply<TWorld>(
            World<TWorld>.Entity entity,
            GameObject host,
            AstarGridGraphConverterSettings settings)
            where TWorld : struct, IWorldType
        {
            entity.Set(new AstarPathComponent {
                Backend = host != null ? host.GetComponent<AstarPath>() : null,
            });
            entity.Set(new AstarGridGraphConfigComponent
            {
                Center = settings.center,
                Rotation = settings.rotation,
                Width = settings.width,
                Depth = settings.depth,
                NodeSize = settings.nodeSize,
                ObstacleMask = settings.obstacleMask.value,
                AgentDiameter = settings.agentDiameter,
                AgentHeight = settings.agentHeight,
                FlushGraphUpdates = settings.flushGraphUpdates,
            });
        }
    }
}
