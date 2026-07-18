using unigame.staticecs.unity;
using UnityEngine;

namespace unigame.staticecs.features {
    /// <summary>Main-world alias for <see cref="AstarGridGraphConverter{TWorld}"/>.</summary>
    [AddComponentMenu("Static ECS/Movement/Astar Grid Graph Converter")]
    public sealed class AstarGridGraphConverter : AstarGridGraphConverter<Main> { }
}
