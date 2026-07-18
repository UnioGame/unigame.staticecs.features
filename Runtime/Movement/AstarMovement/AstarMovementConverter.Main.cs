using unigame.staticecs.unity;
using UnityEngine;

namespace unigame.staticecs.features {
    /// <summary>Main-world alias for <see cref="AstarMovementConverter{TWorld}"/>.</summary>
    [AddComponentMenu("Static ECS/Movement/Astar Movement Converter")]
    public sealed class AstarMovementConverter : AstarMovementConverter<Main> { }
}
