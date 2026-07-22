namespace UniGame.StaticEcs.Features
{
    using System;
    using FFS.Libraries.StaticEcs;
    using UniGame.StaticEcs.Unity;
    using UnityEngine;

    /// <summary>Serializable A* grid graph authoring settings.</summary>
    [Serializable]
    public struct AstarGridGraphConverterSettings
    {
        /// <summary>Creates A* grid graph authoring settings.</summary>
        public AstarGridGraphConverterSettings(
            Vector3 center,
            Vector3 rotation,
            int width,
            int depth,
            float nodeSize,
            LayerMask obstacleMask,
            float agentDiameter,
            float agentHeight,
            bool flushGraphUpdates
        )
        {
            this.center = center;
            this.rotation = rotation;
            this.width = width;
            this.depth = depth;
            this.nodeSize = nodeSize;
            this.obstacleMask = obstacleMask;
            this.agentDiameter = agentDiameter;
            this.agentHeight = agentHeight;
            this.flushGraphUpdates = flushGraphUpdates;
        }

        /// <summary>Grid center relative to the graph host.</summary>
        public Vector3 center;

        /// <summary>Grid rotation in Euler angles.</summary>
        public Vector3 rotation;

        /// <summary>Grid width in nodes.</summary>
        public int width;

        /// <summary>Grid depth in nodes.</summary>
        public int depth;

        /// <summary>Size of one graph node.</summary>
        public float nodeSize;

        /// <summary>Physics layers treated as obstacles.</summary>
        public LayerMask obstacleMask;

        /// <summary>Diameter used when calculating walkability.</summary>
        public float agentDiameter;

        /// <summary>Height used when calculating walkability.</summary>
        public float agentHeight;

        /// <summary>Whether queued graph updates are flushed immediately.</summary>
        public bool flushGraphUpdates;

        /// <summary>Returns the default grid graph authoring settings.</summary>
        public static AstarGridGraphConverterSettings Default =>
            new AstarGridGraphConverterSettings(
                Vector3.zero,
                Vector3.zero,
                40,
                40,
                0.5f,
                1 << 6,
                1.4f,
                1f,
                true
            );
    }

    /// <summary>Creates A* backend and grid configuration components from inline data.</summary>
    [Serializable]
    public class AstarGridGraphSerializableConverter<TWorld> : EcsSerializableConverter<TWorld>
        where TWorld : struct, IWorldType
    {
        [SerializeField]
        private AstarGridGraphConverterSettings _settings = AstarGridGraphConverterSettings.Default;

        /// <summary>Gets or sets the grid graph authoring settings.</summary>
        public AstarGridGraphConverterSettings Settings
        {
            get => _settings;
            set => _settings = value;
        }

        /// <inheritdoc />
        public override void Apply(World<TWorld>.Entity entity, GameObject host)
        {
            AstarGridGraphConverterUtility.Apply(entity, host, _settings);
        }
    }
}
