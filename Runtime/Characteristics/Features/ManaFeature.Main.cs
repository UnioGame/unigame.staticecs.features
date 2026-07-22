namespace UniGame.StaticEcs.Features
{
    using Unity;

    public sealed class ManaFeature : ManaFeature<Main>
    {
        public ManaFeature(bool registerRegen = true, short regenOrder = DefaultRegenOrder)
            : base(registerRegen, regenOrder) { }
    }
}
