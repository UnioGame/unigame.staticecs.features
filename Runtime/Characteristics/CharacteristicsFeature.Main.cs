namespace UniGame.StaticEcs.Features
{
    using System;
    using UniGame.StaticEcs.Unity;

    /// <summary>Main-world standard gameplay characteristics feature.</summary>
    [Serializable]
    public sealed class CharacteristicsFeature : CharacteristicsFeature<Main>
    {
        /// <summary>Creates the Main-world characteristics feature with default configuration.</summary>
        public CharacteristicsFeature() { }

        /// <summary>Creates the Main-world characteristics feature.</summary>
        public CharacteristicsFeature(
            bool registerManaRegen = true,
            short manaRegenOrder = ManaFeature.DefaultRegenOrder
        )
            : base(registerManaRegen, manaRegenOrder) { }
    }
}
