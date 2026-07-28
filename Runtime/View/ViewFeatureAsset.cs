namespace UniGame.StaticEcs.Features
{
    using System;
    using System.Collections.Generic;
    using Cysharp.Threading.Tasks;
    using FFS.Libraries.StaticEcs;
    using UniGame.Core.Runtime;
    using UniGame.Core.Runtime.SerializableType;
    using UniGame.StaticEcs.Unity;
    using UniGame.UiSystem.Runtime.Settings;
    using UnityEngine;

    /// <summary>Installs the Main-world View Feature from game-owned View System settings.</summary>
    [CreateAssetMenu(menuName = "Static ECS/Features/View", fileName = nameof(ViewFeatureAsset))]
    public sealed class ViewFeatureAsset :
        StaticEcsFeatureAsset,
        IStaticEcsFeatureTypeRegistrar<Main>
    {
        /// <summary>View System settings used to discover available view models.</summary>
        public ViewSystemSettings viewSystemSettings;

        /// <summary>Scheduling configuration for the lifecycle system.</summary>
        public ViewFeatureConfig config = ViewFeatureConfig.Default;

        /// <summary>Serialized flattened model catalog used before world initialization.</summary>
        [HideInInspector]
        public List<SType> viewModelTypes = new();

        /// <inheritdoc />
        public void RegisterTypes(World<Main>.TypeRegistrar types)
        {
            ViewModelTypeRegistration.RegisterComponents<Main>(types, GetModelTypes());
        }

        /// <inheritdoc />
        protected override UniTask OnInitializeAsync(ILifeTime lifeTime)
        {
            var feature = new ViewFeature(GetModelTypes(), config);
            return feature.InitializeAsync(lifeTime);
        }

        private List<Type> GetModelTypes()
        {
            var result = new List<Type>(viewModelTypes.Count);
            var unique = new HashSet<Type>();
            foreach (var serializedType in viewModelTypes)
            {
                var type = serializedType?.Type;
                if (type != null && unique.Add(type))
                    result.Add(type);
            }

            return result;
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (viewSystemSettings == null)
            {
                return;
            }

            var discovered = new HashSet<Type>();
            AddModels(viewSystemSettings, discovered);
            foreach (var source in viewSystemSettings.sources)
            {
                var reference = source?.viewSourceReference;
                var path = reference == null
                    ? string.Empty
                    : UnityEditor.AssetDatabase.GUIDToAssetPath(reference.AssetGUID);
                var nested = string.IsNullOrEmpty(path)
                    ? null
                    : UnityEditor.AssetDatabase.LoadAssetAtPath<ViewsSettings>(path);
                if (nested != null)
                {
                    AddModels(nested, discovered);
                }
            }

            viewModelTypes.Clear();
            foreach (var type in discovered)
            {
                viewModelTypes.Add(type);
            }
        }

        private static void AddModels(ViewsSettings settings, HashSet<Type> result)
        {
            foreach (var view in settings.Views)
            {
                var type = view?.ViewModelType?.Type;
                if (type != null)
                {
                    result.Add(type);
                }
            }
        }
#endif
    }
}
