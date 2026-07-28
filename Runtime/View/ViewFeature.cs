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

        /// <summary>Creates a View Feature from its flattened View System model catalog.</summary>
        public ViewFeature(
            IReadOnlyList<Type> modelTypes,
            ViewFeatureConfig config)
        {
            _modelTypes = modelTypes ?? throw new ArgumentNullException(nameof(modelTypes));
            _config = config;
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

            var context = StaticEcsContext.Get<TWorld>();
            var viewSystem = await context
                .ReceiveFirstAsync<IGameViewSystem>(lifeTime)
                .AttachExternalCancellation(lifeTime.Token);
            var binders = ViewModelTypeRegistration.CreateBinders<TWorld>(_modelTypes);

            World<TWorld>.Systems<StaticEcsUpdateSystems>.Add(
                new UpdateViewLifecycleSystem<TWorld>(viewSystem, binders),
                _config.lifecycleOrder);
        }
    }
}
