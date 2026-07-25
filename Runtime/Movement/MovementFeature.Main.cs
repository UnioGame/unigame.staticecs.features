namespace UniGame.StaticEcs.Features
{
    using System;
    using Unity;

    /// <summary>Main-world alias for <see cref="MovementFeature{TWorld}"/>.</summary>
    [Serializable]
    public sealed class MovementFeature : MovementFeature<Main> { }
}
