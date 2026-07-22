namespace UniGame.StaticEcs.Features
{
    using Unity;

    public sealed class AbilityFeature : AbilityFeature<Main>
    {
        public AbilityFeature(
            bool registerSystems = true,
            short castOrder = DefaultCastOrder,
            short waitOrder = DefaultWaitOrder,
            short progressionOrder = DefaultProgressionOrder
        )
            : base(registerSystems, castOrder, waitOrder, progressionOrder) { }
    }
}
