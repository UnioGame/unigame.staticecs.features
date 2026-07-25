namespace UniGame.StaticEcs.Features
{
    using UniGame.StaticEcs.Unity;

    /// <summary>Creates exact-interface Main-world random resources.</summary>
    public static class UnityAbilityRng
    {
        /// <summary>Creates the Main-world random source.</summary>
        public static UnityAbilityRng<Main> Create() => new();
    }
}
