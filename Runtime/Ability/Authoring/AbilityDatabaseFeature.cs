namespace UniGame.StaticEcs.Features
{
    using System;
    using Cysharp.Threading.Tasks;
    using FFS.Libraries.StaticEcs;
    using UniGame.Core.Runtime;
    using UniGame.StaticEcs.Unity;

    /// <summary>Publishes an authored ability database and installs its initialization system.</summary>
    public class AbilityDatabaseFeature<TWorld> : StaticEcsFeature<TWorld>
        where TWorld : struct, IWorldType
    {
        private const short InitializationOrder = short.MinValue;

        /// <summary>Authored database registered during world startup.</summary>
        public AbilityDatabase database;

        /// <summary>Whether ability assets are cloned for the runtime world.</summary>
        public bool instantiateAssets = true;

        /// <inheritdoc />
        public override async UniTask InitializeAsync(ILifeTime lifeTime)
        {
            World<TWorld>.Resource<AbilityRegistry<TWorld>> registry = default;
            await registry.GetAsync(lifeTime);

            if (!World<TWorld>.HasResource<AbilityDatabaseConfig>())
            {
                var config = new AbilityDatabaseConfig
                {
                    Database = database,
                    InstantiateAssets = instantiateAssets,
                };

                World<TWorld>.SetResource(config);
            }

            var systemsConfig = World<TWorld>
                .GetResource<Unity.StaticEcsSystemsConfig>();
            if (!systemsConfig.update)
            {
                throw new InvalidOperationException(
                    "Ability database initialization requires the Static ECS Update group.");
            }

            World<TWorld>.Systems<StaticEcsUpdateSystems>.Add(
                new AbilityDatabaseInitializationSystem<TWorld>(),
                InitializationOrder);
        }
    }

    /// <summary>References the authored ability database used during world startup.</summary>
    public sealed class AbilityDatabaseConfig : IResource
    {
        /// <summary>Authored database registered during world startup.</summary>
        public AbilityDatabase Database;

        /// <summary>Whether ability assets are cloned and owned by the runtime feature.</summary>
        public bool InstantiateAssets = true;
    }
}
