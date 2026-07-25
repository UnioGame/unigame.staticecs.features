namespace UniGame.StaticEcs.Features
{
    using System;
    using Unity;

    /// <summary>
    /// Main-default alias for <see cref="DamageFeature{TWorld}"/>. See the world-default
    /// aliases convention for usage rules.
    /// </summary>
    [Serializable]
    public sealed class DamageFeature : DamageFeature<Main>
    { }
}
