namespace UniGame.StaticEcs.Features
{
    using UniGame.StaticEcs.Unity;

    /// <summary>Creates exact-interface Main-world immobilize handlers.</summary>
    public static class ImmobilizeEffectHandler
    {
        /// <summary>Creates the Main-world handler.</summary>
        public static ImmobilizeEffectHandler<Main> Create() => new();
    }
}
