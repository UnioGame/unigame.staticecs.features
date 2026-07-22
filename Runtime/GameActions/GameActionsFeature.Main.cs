namespace UniGame.StaticEcs.Features
{
    using System;
    using Unity;

    /// <summary>Main-world alias for <see cref="GameActionsFeature{TWorld}"/>.</summary>
    [Serializable]
    public sealed class GameActionsFeature : GameActionsFeature<Main>
    {
        /// <summary>Creates the Main-world action mask feature with default configuration.</summary>
        public GameActionsFeature() { }

        /// <summary>Creates the Main-world action mask feature.</summary>
        public GameActionsFeature(bool registerMaintenance = true, short maintenanceOrder = 25)
            : base(registerMaintenance, maintenanceOrder) { }
    }
}
