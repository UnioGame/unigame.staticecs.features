namespace UniGame.StaticEcs.Features
{
    using System;
    using System.Collections.Generic;
    using FFS.Libraries.StaticEcs;
    using UnityEngine;
    using Object = UnityEngine.Object;

    /// <summary>Registers authored abilities and owns their optional runtime clones.</summary>
    public class AbilityDatabaseInitializationSystem<TWorld> : ISystem
        where TWorld : struct, IWorldType
    {
        private readonly List<AbilityAsset> _runtimeInstances = new();

        /// <inheritdoc />
        public void Init()
        {
            if (!World<TWorld>.HasResource<AbilityDatabaseConfig>())
            {
                return;
            }

            ref var config = ref World<TWorld>.GetResource<AbilityDatabaseConfig>();
            var database = config.Database;
            if (database == null)
            {
                return;
            }

            var registry = World<TWorld>.GetResource<AbilityRegistry<TWorld>>();
            var ids = new HashSet<int>();
            try
            {
                for (var i = 0; i < database.Count; i++)
                {
                    var source = database.GetAbility(i);
                    if (source == null)
                    {
                        continue;
                    }

                    var asset = config.InstantiateAssets ? Object.Instantiate(source) : source;
                    if (config.InstantiateAssets)
                    {
                        _runtimeInstances.Add(asset);
                    }

                    var id = asset.Id;
                    if (!ids.Add(id.Value))
                    {
                        throw new InvalidOperationException(
                            $"Ability database contains duplicate ability id {id}."
                        );
                    }

                    if (registry.Contains(id))
                    {
                        throw new InvalidOperationException(
                            $"Ability registry already contains ability id {id}."
                        );
                    }

                    if (asset.Root == null)
                    {
                        throw new InvalidOperationException(
                            $"Ability asset {asset.name} has no root step."
                        );
                    }

                    registry.Register(asset.BuildDefinition(), asset.Root);
                }
            }
            catch
            {
                DestroyRuntimeInstances();
                throw;
            }
        }

        /// <inheritdoc />
        public void Destroy()
        {
            DestroyRuntimeInstances();
        }

        private void DestroyRuntimeInstances()
        {
            for (var i = _runtimeInstances.Count - 1; i >= 0; i--)
            {
                var asset = _runtimeInstances[i];
                if (asset == null)
                {
                    continue;
                }

                if (Application.isPlaying)
                {
                    Object.Destroy(asset);
                }
                else
                {
                    Object.DestroyImmediate(asset);
                }
            }

            _runtimeInstances.Clear();
        }
    }
}
