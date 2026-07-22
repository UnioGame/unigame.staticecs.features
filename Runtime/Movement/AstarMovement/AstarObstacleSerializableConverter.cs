namespace UniGame.StaticEcs.Features
{
    using System;
    using FFS.Libraries.StaticEcs;
    using FFS.Libraries.StaticEcs.Unity;
    using UniGame.StaticEcs.Unity;
    using UnityEngine;

    /// <summary>Creates and resolves an A* obstacle from inline authoring data.</summary>
    [Serializable]
    public class AstarObstacleSerializableConverter<TWorld>
        : EcsSerializableConverter<TWorld>,
            IEcsLinkResolver<TWorld>
        where TWorld : struct, IWorldType
    {
        [SerializeField]
        private AbstractStaticEcsEntityProvider _graphProvider;

        /// <summary>Gets or sets the provider owning the A* graph entity.</summary>
        public AbstractStaticEcsEntityProvider GraphProvider
        {
            get => _graphProvider;
            set => _graphProvider = value;
        }

        /// <inheritdoc />
        public override void Apply(World<TWorld>.Entity entity, GameObject host)
        {
            AstarObstacleConverterUtility.Apply(entity, host, _graphProvider);
        }

        /// <inheritdoc />
        public void ResolveLinks(World<TWorld>.Entity entity, GameObject host)
        {
            AstarObstacleConverterUtility.ResolveLinks(entity, _graphProvider);
        }
    }
}
