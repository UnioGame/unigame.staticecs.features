namespace UniGame.StaticEcs.Features
{
    using UniGame.StaticEcs.Unity;

    /// <summary>Main-world standard gameplay characteristics feature.</summary>
    public sealed class CharacteristicsFeature : CharacteristicsFeature<Main>
    {
        /// <summary>Creates the Main-world characteristics feature.</summary>
        public CharacteristicsFeature(
            bool registerManaRegen = true,
            short manaRegenOrder = ManaFeature.DefaultRegenOrder
        )
            : base(registerManaRegen, manaRegenOrder) { }
    }
}
