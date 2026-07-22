namespace UniGame.StaticEcs.Features
{
    using System;
    using System.Collections.Generic;
    using FFS.Libraries.StaticEcs;

    public class AbilityDatabaseFeature<TWorld> : StaticEcsFeature<TWorld>
        where TWorld : struct, IWorldType
    {
        private readonly AbilityDatabase _database;
        private readonly bool _instantiateAssets;
        private readonly List<AbilityAsset> _runtimeInstances = new();

        public AbilityDatabaseFeature(AbilityDatabase database, bool instantiateAssets = true)
        {
            _database = database;
            _instantiateAssets = instantiateAssets;
        }

        public override void RegisterTypes(World<TWorld>.TypeRegistrar types)
        {
            if (_database == null)
            {
                return;
            }

            if (!World<TWorld>.HasResource<AbilityRegistry<TWorld>>())
            {
                World<TWorld>.SetResource(new AbilityRegistry<TWorld>());
            }

            var registry = World<TWorld>.GetResource<AbilityRegistry<TWorld>>();
            var ids = new HashSet<int>();
            _runtimeInstances.Clear();

            for (var i = 0; i < _database.Count; i++)
            {
                var source = _database.GetAbility(i);
                if (source == null)
                {
                    continue;
                }

                var asset = _instantiateAssets ? UnityEngine.Object.Instantiate(source) : source;
                if (_instantiateAssets)
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
    }
}
