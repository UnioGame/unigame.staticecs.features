using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;

namespace UniGame.StaticEcs.Features.Editor.AbilityGraph {
    internal static class AbilityGraphNodeTypeRegistry {
        internal sealed class Entry {
            public Type Type;
            public string DisplayName;
            public string Category;
            public string SearchText;
        }

        private static List<Entry> _entries;

        public static IReadOnlyList<Entry> GetEntries() {
            if (_entries != null) {
                return _entries;
            }

            _entries = new List<Entry>();
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            for (var assemblyIndex = 0; assemblyIndex < assemblies.Length; assemblyIndex++) {
                var types = GetTypesSafe(assemblies[assemblyIndex]);
                for (var typeIndex = 0; typeIndex < types.Count; typeIndex++) {
                    var type = types[typeIndex];
                    if (!IsCreatableNodeType(type)) {
                        continue;
                    }

                    var metadata = type.GetCustomAttribute<AbilityStepEditorAttribute>(false);
                    var displayName = !string.IsNullOrWhiteSpace(metadata?.DisplayName)
                        ? metadata.DisplayName
                        : ObjectNames.NicifyVariableName(type.Name.Replace("StepConfig", string.Empty));
                    var category = string.IsNullOrWhiteSpace(metadata?.Category)
                        ? "Other"
                        : metadata.Category;

                    _entries.Add(new Entry {
                        Type = type,
                        DisplayName = displayName,
                        Category = category,
                        SearchText = $"{displayName} {category} {type.FullName}".ToLowerInvariant(),
                    });
                }
            }

            _entries = _entries
                .OrderBy(x => x.Category, StringComparer.OrdinalIgnoreCase)
                .ThenBy(x => x.DisplayName, StringComparer.OrdinalIgnoreCase)
                .ToList();

            return _entries;
        }

        private static bool IsCreatableNodeType(Type type) {
            if (type == null || !typeof(IAbilityStepConfig).IsAssignableFrom(type)) {
                return false;
            }

            if (type.IsAbstract || type.IsInterface || type.IsGenericTypeDefinition) {
                return false;
            }

            if (!type.IsDefined(typeof(SerializableAttribute), false)) {
                return false;
            }

            return type.GetConstructor(Type.EmptyTypes) != null;
        }

        private static IReadOnlyList<Type> GetTypesSafe(Assembly assembly) {
            try {
                return assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException exception) {
                return exception.Types;
            }
        }
    }
}