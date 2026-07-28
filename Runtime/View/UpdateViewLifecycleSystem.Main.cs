namespace UniGame.StaticEcs.Features
{
    using UniGame.StaticEcs.Unity;
    using UniGame.ViewSystem.Runtime;

    /// <summary>Main-world View System lifecycle orchestration system.</summary>
    public sealed class UpdateViewLifecycleSystem :
        UpdateViewLifecycleSystem<Main>
    {
        internal UpdateViewLifecycleSystem(
            IGameViewSystem viewSystem,
            ViewModelBinderRegistry<Main> binders)
            : base(viewSystem, binders)
        {
        }
    }
}
