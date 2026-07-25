namespace UniGame.StaticEcs.Features
{
    using System;
    using Unity;

    /// <summary>Main-world alias for <see cref="GameActionsFeature{TWorld}"/>.</summary>
    [Serializable]
    public sealed class GameActionsFeature : GameActionsFeature<Main>
    {
    }
}
