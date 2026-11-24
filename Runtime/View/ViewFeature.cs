namespace UniGame.StaticEcs.Features
{
    using System;
    using System.Collections.Generic;
    using Cysharp.Threading.Tasks;
    using FFS.Libraries.StaticEcs;
    using UniGame.Context.Runtime;
    using UniGame.Core.Runtime;
    using UniGame.StaticEcs.Unity;
    using UniGame.ViewSystem.Runtime;

    /// <summary>Connects Static ECS view entities directly to the shared View System runtime.</summary>
    public class ViewFeature<TWorld> : StaticEcsFeature<TWorld>
        where TWorld : struct, IWorldType
    {
        private readonly IReadOnlyList<Type> _modelTypes;
        private readonly ViewFeatureConfig _config;
        private readonly IGameViewSystem _viewSystem;

        /// <summary>Creates a View Feature from its flattened View System model catalog.</summary>
        public ViewFeature(
            IReadOnlyList<Type> modelTypes,
            ViewFeatureConfig config)
        {
            _modelTypes = modelTypes ?? throw new ArgumentNullException(nameof(modelTypes));
            _config = config;
        }

        /// <summary>Creates a View Feature bound to a specific View System instance.</summary>
        public ViewFeature(
            IReadOnlyList<Type> modelTypes,
            ViewFeatureConfig config,
            IGameViewSystem viewSystem)
            : this(modelTypes, config)
        {
            _viewSystem = viewSystem ?? throw new ArgumentNullException(nameof(viewSystem));
        }

        /// <inheritdoc />
        public override async UniTask InitializeAsync(ILifeTime lifeTime)
        {
            if (!World<TWorld>.HasResource<ViewKeySequenceResource<TWorld>>())
                World<TWorld>.SetResource(new ViewKeySequenceResource<TWorld>());

            if (!World<TWorld>.HasResource<ViewContainerRegistryResource<TWorld>>())
            {
                var containers = new ViewContainerRegistryResource<TWorld>();
                World<TWorld>.SetResource(containers);
            }

            if (!World<TWorld>.HasResource<UiToolkitElementRegistryResource<TWorld>>())
            {
                var elements = new UiToolkitElementRegistryResource<TWorld>();
                World<TWorld>.SetResource(elements);
            }

            var viewSystem = _viewSystem;
            if (viewSystem == null)
            {
                var context = StaticEcsContext.Get<TWorld>();
                viewSystem = await context
                    .ReceiveFirstAsync<IGameViewSystem>(lifeTime)
                    .AttachExternalCancellation(lifeTime.Token);
            }
            var binders = ViewModelTypeRegistration.CreateBinders<TWorld>(_modelTypes);

            World<TWorld>.Systems<StaticEcsUpdateSystems>.Add(
                new UpdateViewLifecycleSystem<TWorld>(viewSystem, binders),
                _config.lifecycleOrder);
            World<TWorld>.Systems<StaticEcsUpdateSystems>.Add(
                new SyncUiToolkitElementsSystem<TWorld>(),
                _config.lifecycleOrder < short.MaxValue
                    ? (short)(_config.lifecycleOrder + 1)
                    : _config.lifecycleOrder);
        }
    }
}
