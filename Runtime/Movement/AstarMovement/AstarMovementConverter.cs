using FFS.Libraries.StaticEcs;
using FFS.Libraries.StaticEcs.Unity;
using Pathfinding;
 
using UnityEngine;

namespace UniGame.StaticEcs.Features {
    using Unity;

    /// <summary>
    /// Sets <see cref="AstarAIComponent"/> on conversion by reading <see cref="IAstarAI"/>
    /// from the host <see cref="GameObject"/>.
    /// </summary>
    public class AstarMovementConverter<TWorld> : EcsMonoConverter<TWorld>, IEcsLinkResolver<TWorld>
        where TWorld : struct, IWorldType {
        [SerializeField] private AbstractStaticEcsEntityProvider _graphProvider;

        /// <inheritdoc/>
        public override void Apply(World<TWorld>.Entity entity, GameObject host) {
            var ai = host != null ? host.GetComponent<IAstarAI>() : null;
            entity.Set(new AstarAIComponent {
                AI = ai,
                Seeker = host != null ? host.GetComponent<Seeker>() : null,
                GraphEntity = _graphProvider != null ? _graphProvider.EntityGid : default,
            });
        }

        /// <inheritdoc/>
        public void ResolveLinks(World<TWorld>.Entity entity, GameObject host) {
            if (!entity.Has<AstarAIComponent>()) {
                return;
            }

            entity.Mut<AstarAIComponent>().GraphEntity = _graphProvider != null
                ? _graphProvider.EntityGid
                : default;
        }
    }
}
