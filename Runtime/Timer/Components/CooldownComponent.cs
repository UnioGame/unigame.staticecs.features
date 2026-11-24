namespace UniGame.StaticEcs.Features
{
    using FFS.Libraries.StaticEcs;

    /// <summary>Configures one entity cooldown duration in milliseconds of scaled ECS time.</summary>
    public struct CooldownComponent : IComponent
    {
        public uint DurationMilliseconds;
    }
}
