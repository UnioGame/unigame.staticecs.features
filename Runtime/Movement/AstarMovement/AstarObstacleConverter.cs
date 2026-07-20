using FFS.Libraries.StaticEcs;
using FFS.Libraries.StaticEcs.Unity;
 
using UnityEngine;

namespace UniGame.StaticEcs.Features {
    using Unity;

    /// <summary>Converts a collider into an ECS obstacle linked to a graph entity provider.</summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Collider))]
    public class AstarObstacleConverter<TWorld> : EcsMonoConverter<TWorld>, IEcsLinkResolver<TWorld>
        where TWorld : struct, IWorldType {
        [SerializeField] private AbstractStaticEcsEntityProvider _graphProvider;

        /// <inheritdoc/>
        public override void Apply(World<TWorld>.Entity entity, GameObject host) {
            AstarObstacleConverterUtility.Apply(entity, host, _graphProvider);
        }

        /// <inheritdoc/>
        public void ResolveLinks(World<TWorld>.Entity entity, GameObject host) {
            AstarObstacleConverterUtility.ResolveLinks(entity, _graphProvider);
        }
    }

    internal static class AstarObstacleConverterUtility {
        public static void Apply<TWorld>(
            World<TWorld>.Entity entity,
            GameObject host,
            AbstractStaticEcsEntityProvider graphProvider)
            where TWorld : struct, IWorldType {
            entity.Set(new AstarObstacleComponent {
                Collider = host != null ? host.GetComponent<Collider>() : null,
                GraphProvider = graphProvider,
                GraphEntity = graphProvider != null ? graphProvider.EntityGid : default,
            });
        }

        public static void ResolveLinks<TWorld>(
            World<TWorld>.Entity entity,
            AbstractStaticEcsEntityProvider graphProvider)
            where TWorld : struct, IWorldType {
            if (!entity.Has<AstarObstacleComponent>()) {
                return;
            }

            ref var obstacle = ref entity.Mut<AstarObstacleComponent>();
            obstacle.GraphProvider = graphProvider;
            obstacle.GraphEntity = graphProvider != null ? graphProvider.EntityGid : default;
        }
    }
}
