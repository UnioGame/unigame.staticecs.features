namespace UniGame.StaticEcs.Features
{
    using System;
    using Unity;

    /// <summary>Main-world alias for <see cref="AstarMovementFeature{TWorld}"/>.</summary>
    [Serializable]
    public sealed class AstarMovementFeature : AstarMovementFeature<Main>
    { }
}
