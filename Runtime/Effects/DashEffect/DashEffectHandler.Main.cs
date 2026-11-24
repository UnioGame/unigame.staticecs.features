namespace UniGame.StaticEcs.Features
{
    using UniGame.StaticEcs.Unity;

    /// <summary>Creates exact-interface Main-world dash handlers.</summary>
    public static class DashEffectHandler
    {
        /// <summary>Creates the Main-world handler.</summary>
        public static DashEffectHandler<Main> Create() => new();
    }
}
