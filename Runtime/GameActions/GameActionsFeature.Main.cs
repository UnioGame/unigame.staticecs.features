namespace UniGame.StaticEcs.Features
{
    using Unity;

    /// <summary>Main-world alias for <see cref="GameActionsFeature{TWorld}"/>.</summary>
    public sealed class GameActionsFeature : GameActionsFeature<Main>
    {
        /// <summary>Creates the Main-world action mask feature.</summary>
        public GameActionsFeature(bool registerMaintenance = true, short maintenanceOrder = 25)
            : base(registerMaintenance, maintenanceOrder) { }
    }
}
