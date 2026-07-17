using FFS.Libraries.StaticEcs;
using Pathfinding;
using unigame.staticecs.unity;
using UnityEngine;

namespace unigame.staticecs.features {
    /// <summary>Main-world alias for <see cref="AstarMovementConverter{TWorld}"/>.</summary>
    [AddComponentMenu("Static ECS/Movement/Astar Movement Converter")]
    public sealed class AstarMovementConverter : AstarMovementConverter<Main> { }

    /// <summary>
    /// Sets <see cref="AstarAIComponent"/> on conversion by reading <see cref="IAstarAI"/>
    /// from the host <see cref="GameObject"/>.
    /// </summary>
    public class AstarMovementConverter<TWorld> : EcsMonoConverter<TWorld>
        where TWorld : struct, IWorldType {
        /// <inheritdoc/>
        public override void Apply(World<TWorld>.Entity entity, GameObject host) {
            var ai = host != null ? host.GetComponent<IAstarAI>() : null;
            entity.Set(new AstarAIComponent { AI = ai });
        }
    }
}
