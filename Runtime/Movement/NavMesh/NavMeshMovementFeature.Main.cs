namespace UniGame.StaticEcs.Features
{
    using System;
    using Unity;

    /// <summary>Main-world alias for <see cref="NavMeshMovementFeature{TWorld}"/>.</summary>
    [Serializable]
    public sealed class NavMeshMovementFeature : NavMeshMovementFeature<Main> { }
}
