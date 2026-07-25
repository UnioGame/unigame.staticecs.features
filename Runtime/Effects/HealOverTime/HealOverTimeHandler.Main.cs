namespace UniGame.StaticEcs.Features
{
    using UniGame.StaticEcs.Unity;

    /// <summary>Creates exact-interface Main-world heal-over-time handlers.</summary>
    public static class HealOverTimeHandler
    {
        /// <summary>Creates the Main-world handler.</summary>
        public static HealOverTimeHandler<Main> Create() => new();
    }
}
