using FFS.Libraries.StaticEcs;
using FFS.Libraries.StaticEcs.Unity;
using unigame.staticecs.unity;
using UnityEngine;

namespace unigame.staticecs.features {
    /// <summary>Converts a collider into an ECS obstacle linked to a graph entity provider.</summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Collider))]
    public class AstarObstacleConverter<TWorld> : EcsMonoConverter<TWorld>, IEcsLinkResolver<TWorld>
        where TWorld : struct, IWorldType {
        [SerializeField] private AbstractStaticEcsEntityProvider _graphProvider;

        /// <inheritdoc/>
        public override void Apply(World<TWorld>.Entity entity, GameObject host) {
            entity.Set(new AstarObstacleComponent {
                Collider = host != null ? host.GetComponent<Collider>() : null,
                GraphProvider = _graphProvider,
                GraphEntity = _graphProvider != null ? _graphProvider.EntityGid : default,
            });
        }

        /// <inheritdoc/>
        public void ResolveLinks(World<TWorld>.Entity entity, GameObject host) {
            if (!entity.Has<AstarObstacleComponent>()) {
                return;
            }

            ref var obstacle = ref entity.Mut<AstarObstacleComponent>();
            obstacle.GraphProvider = _graphProvider;
            obstacle.GraphEntity = _graphProvider != null ? _graphProvider.EntityGid : default;
        }
    }
}
