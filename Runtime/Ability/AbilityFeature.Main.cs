namespace UniGame.StaticEcs.Features
{
    using System;
    using Unity;

    /// <summary>Main-world ability execution feature.</summary>
    [Serializable]
    public sealed class AbilityFeature : AbilityFeature<Main>
    {
        /// <summary>Creates the Main-world ability feature with default configuration.</summary>
        public AbilityFeature() { }

        /// <summary>Creates the Main-world ability feature.</summary>
        public AbilityFeature(
            bool registerSystems = true,
            short castOrder = DefaultCastOrder,
            short waitOrder = DefaultWaitOrder,
            short progressionOrder = DefaultProgressionOrder
        )
            : base(registerSystems, castOrder, waitOrder, progressionOrder) { }
    }
}
