using System.Collections.Generic;
using FFS.Libraries.StaticEcs;
 
using UnityEditor;
using UnityEngine;

namespace UniGame.StaticEcs.Features.Editor.AbilityGraph {
    using Unity;

    internal sealed class AbilityGraphRuntimeBrowser {
        internal sealed class RuntimeAbilityEntry {
            public AbilityAsset Asset;
            public AbilityId AbilityId;
            public string DisplayName;
            public EntityGID Owner;
            public EntityGID Cast;
            public string ActiveNodeGuid;
            public bool IsActiveCast;
            public GameObject OwnerGameObject;
        }

        private readonly Dictionary<int, AbilityAsset> _assetById = new();
        private readonly List<RuntimeAbilityEntry> _activeCasts = new();
        private readonly List<RuntimeAbilityEntry> _equippedAbilities = new();

        public IReadOnlyList<RuntimeAbilityEntry> ActiveCasts => _activeCasts;
        public IReadOnlyList<RuntimeAbilityEntry> EquippedAbilities => _equippedAbilities;

        public void Refresh() {
            _activeCasts.Clear();
            _equippedAbilities.Clear();
            RebuildAssetIndex();

            if (World<Main>.Status != WorldStatus.Initialized) {
                return;
            }

            CollectActiveCasts();
            CollectEquippedAbilities();
        }

        private void RebuildAssetIndex() {
            _assetById.Clear();

            var guids = AssetDatabase.FindAssets("t:AbilityAsset");
            for (var i = 0; i < guids.Length; i++) {
                var path = AssetDatabase.GUIDToAssetPath(guids[i]);
                var asset = AssetDatabase.LoadAssetAtPath<AbilityAsset>(path);
                if (asset == null) {
                    continue;
                }

                if (!_assetById.ContainsKey(asset.Id.Value)) {
                    _assetById.Add(asset.Id.Value, asset);
                }
            }
        }

        private void CollectActiveCasts() {
            AbilityRegistry<Main> registry = null;
            if (World<Main>.HasResource<AbilityRegistry<Main>>()) {
                registry = World<Main>.GetResource<AbilityRegistry<Main>>();
            }

            foreach (var entity in World<Main>
                         .Query<All<AbilityCastRuntimeComponent, AbilityCastOwnerRef>>()
                         .Entities()) {
                var runtime = entity.Read<AbilityCastRuntimeComponent>();
                var owner = entity.Read<AbilityCastOwnerRef>().Owner;

                var entry = new RuntimeAbilityEntry {
                    Asset = ResolveAsset(runtime.AbilityId),
                    AbilityId = runtime.AbilityId,
                    DisplayName = ResolveDisplayName(runtime.AbilityId, registry),
                    Owner = owner,
                    Cast = entity.GID,
                    ActiveNodeGuid = ResolveActiveNodeGuid(entity),
                    IsActiveCast = true,
                    OwnerGameObject = ResolveOwnerGameObject(owner),
                };

                _activeCasts.Add(entry);
            }
        }

        private void CollectEquippedAbilities() {
            AbilityRegistry<Main> registry = null;
            if (World<Main>.HasResource<AbilityRegistry<Main>>()) {
                registry = World<Main>.GetResource<AbilityRegistry<Main>>();
            }

            foreach (var entity in World<Main>
                         .Query<All<World<Main>.Multi<AbilityRosterEntry>>>()
                         .Entities()) {
                ref readonly var roster = ref entity.Read<World<Main>.Multi<AbilityRosterEntry>>();
                for (var i = 0; i < roster.Length; i++) {
                    var abilityId = roster.Get(i).Id;
                    _equippedAbilities.Add(new RuntimeAbilityEntry {
                        Asset = ResolveAsset(abilityId),
                        AbilityId = abilityId,
                        DisplayName = ResolveDisplayName(abilityId, registry),
                        Owner = entity.GID,
                        Cast = default,
                        ActiveNodeGuid = string.Empty,
                        IsActiveCast = false,
                        OwnerGameObject = ResolveOwnerGameObject(entity.GID),
                    });
                }
            }
        }

        private AbilityAsset ResolveAsset(AbilityId abilityId) {
            return _assetById.TryGetValue(abilityId.Value, out var asset) ? asset : null;
        }

        private static string ResolveDisplayName(AbilityId abilityId, AbilityRegistry<Main> registry) {
            if (registry != null && registry.TryGet(abilityId, out var definition, out _)) {
                if (!string.IsNullOrWhiteSpace(definition.DisplayName)) {
                    return definition.DisplayName;
                }
            }

            return abilityId.ToString();
        }

        private static string ResolveActiveNodeGuid(World<Main>.Entity castEntity) {
            if (!castEntity.Has<World<Main>.Multi<AbilityActiveStepEntry>>()) {
                return string.Empty;
            }

            ref readonly var activeSteps = ref castEntity.Read<World<Main>.Multi<AbilityActiveStepEntry>>();
            if (activeSteps.Length == 0) {
                return string.Empty;
            }

            return activeSteps.Get(0).NodeGuid ?? string.Empty;
        }

        private static GameObject ResolveOwnerGameObject(EntityGID owner) {
            if (!owner.TryUnpack<Main>(out var ownerEntity)) {
                return null;
            }
            if (!ownerEntity.Has<TransformBindingComponent>()) {
                return null;
            }

            var transform = ownerEntity.Read<TransformBindingComponent>().Transform;
            return transform != null ? transform.gameObject : null;
        }
    }
}
