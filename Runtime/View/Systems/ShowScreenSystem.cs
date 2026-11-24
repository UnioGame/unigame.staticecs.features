namespace UniGame.StaticEcs.Features
{
    using FFS.Libraries.StaticEcs;
    using UniGame.StaticEcs.Unity;
    using UniGame.ViewSystem.Runtime;

    /// <summary>Consumes request components and opens their configured View System screen.</summary>
    public class ShowScreenSystem<TWorld, TComponent, TView, TViewModel> : ISystem
        where TWorld : struct, IWorldType
        where TComponent : struct, IComponent
        where TView : IView
        where TViewModel : class, IViewModel
    {
        private readonly ViewRequest _request;

        /// <summary>Creates a screen opener from placement and ownership settings.</summary>
        public ShowScreenSystem(ViewRequest request)
        {
            _request = ViewRequest.Bind<TView, TViewModel>(request);
        }

        /// <inheritdoc />
        public void Update()
        {
            var filter = default(All<TComponent>);
            foreach (World<TWorld>.Entity entity in World<TWorld>.Query(filter).Entities())
            {
                var request = _request;
                if (request.Owner == default)
                    request.Owner = entity.GID;
                if (request.Source == default)
                    request.Source = entity.GID;

                if (!ViewOperations.TryOpenNew<TWorld>(request, out _))
                    continue;

                entity.Delete<TComponent>();
            }
        }
    }
}
