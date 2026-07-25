namespace UniGame.StaticEcs.Features
{
    using UniGame.StaticEcs.Unity;

    /// <summary>Creates exact-interface Main-world characteristic modification handlers.</summary>
    public static class ModificationEffectHandler<TStat>
        where TStat : struct, ICharacteristicType
    {
        /// <summary>Creates the Main-world handler.</summary>
        public static ModificationEffectHandler<Main, TStat> Create() => new();
    }
}
