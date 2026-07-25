namespace UniGame.StaticEcs.Features
{
    using UniGame.StaticEcs.Unity;

    /// <summary>Creates exact-interface Main-world stun handlers.</summary>
    public static class StunEffectHandler
    {
        /// <summary>Creates the Main-world handler.</summary>
        public static StunEffectHandler<Main> Create() => new();
    }
}
