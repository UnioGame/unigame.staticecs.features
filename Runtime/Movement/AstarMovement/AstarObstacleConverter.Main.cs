namespace UniGame.StaticEcs.Features
{
    using Unity;
    using UnityEngine;

    /// <summary>Main-world alias for <see cref="AstarObstacleConverter{TWorld}"/>.</summary>
    [AddComponentMenu("Static ECS/Movement/Astar Obstacle Converter")]
    public sealed class AstarObstacleConverter : AstarObstacleConverter<Main> { }
}
