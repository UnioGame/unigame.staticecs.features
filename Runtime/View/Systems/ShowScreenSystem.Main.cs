namespace UniGame.StaticEcs.Features
{
    using FFS.Libraries.StaticEcs;
    using UniGame.StaticEcs.Unity;
    using UniGame.ViewSystem.Runtime;

    /// <summary>Main-world form of the typed screen opener.</summary>
    public sealed class ShowScreenSystem<TComponent, TView, TViewModel> :
        ShowScreenSystem<Main, TComponent, TView, TViewModel>
        where TComponent : struct, IComponent
        where TView : IView
        where TViewModel : class, IViewModel
    {
        /// <summary>Creates a Main-world screen opener.</summary>
        public ShowScreenSystem(ViewRequest request)
            : base(request)
        {
        }
    }
}
