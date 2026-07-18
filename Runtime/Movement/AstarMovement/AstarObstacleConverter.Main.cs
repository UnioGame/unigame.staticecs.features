using unigame.staticecs.unity;
using UnityEngine;

namespace unigame.staticecs.features {
    /// <summary>Main-world alias for <see cref="AstarObstacleConverter{TWorld}"/>.</summary>
    [AddComponentMenu("Static ECS/Movement/Astar Obstacle Converter")]
    public sealed class AstarObstacleConverter : AstarObstacleConverter<Main> { }
}
