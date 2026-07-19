using FFS.Libraries.StaticEcs;
using UnityEngine;
using UnityEngine.AI;

namespace UniGame.StaticEcs.Features {
    using Unity;

    /// <summary>Main-world alias for <see cref="NavMeshMovementConverter{TWorld}"/>.</summary>
    [AddComponentMenu("Static ECS/Movement/NavMesh Movement Converter")]
    public sealed class NavMeshMovementConverter : NavMeshMovementConverter<Main> { }

    /// <summary>
    /// Sets <see cref="NavMeshAgentComponent"/> on conversion by reading the
    /// <see cref="NavMeshAgent"/> from the host <see cref="GameObject"/>.
    /// </summary>
    public class NavMeshMovementConverter<TWorld> : EcsMonoConverter<TWorld>
        where TWorld : struct, IWorldType {
        /// <inheritdoc/>
        public override void Apply(World<TWorld>.Entity entity, GameObject host) {
            var agent = host != null ? host.GetComponent<NavMeshAgent>() : null;
            entity.Set(new NavMeshAgentComponent { Agent = agent });
        }
    }
}
