using System;
using UnityEditor;
using UnityEngine;

namespace unigame.staticecs.features.Editor.AbilityGraph {
    internal static class AbilityGraphAssetEditing {
        public static bool ReplaceRoot(AbilityAsset asset, AbilityGraphNodeTypeRegistry.Entry entry) {
            if (!TryCreateNode(entry, out var node)) {
                return false;
            }

            Undo.RecordObject(asset, "Replace Ability Root Node");

            var serializedObject = new SerializedObject(asset);
            serializedObject.Update();

            var rootProperty = serializedObject.FindProperty("_root");
            rootProperty.managedReferenceValue = node;
            serializedObject.ApplyModifiedProperties();

            EditorUtility.SetDirty(asset);
            return true;
        }

        public static bool AppendChild(AbilityAsset asset, IAbilityStepConfig parentNode, string listFieldName, AbilityGraphNodeTypeRegistry.Entry entry) {
            if (!TryCreateNode(entry, out var node)) {
                return false;
            }

            var serializedObject = new SerializedObject(asset);
            serializedObject.Update();

            var rootProperty = serializedObject.FindProperty("_root");
            var parentProperty = FindManagedReferenceProperty(rootProperty, parentNode);
            if (parentProperty == null) {
                return false;
            }

            var listProperty = parentProperty.FindPropertyRelative(listFieldName);
            if (listProperty == null || !listProperty.isArray) {
                return false;
            }

            Undo.RecordObject(asset, "Add Ability Graph Child Node");
            var index = listProperty.arraySize;
            listProperty.InsertArrayElementAtIndex(index);
            var elementProperty = listProperty.GetArrayElementAtIndex(index);
            elementProperty.managedReferenceValue = node;
            serializedObject.ApplyModifiedProperties();

            EditorUtility.SetDirty(asset);
            return true;
        }

        public static bool AssignChild(AbilityAsset asset, IAbilityStepConfig parentNode, string childFieldName, AbilityGraphNodeTypeRegistry.Entry entry) {
            if (!TryCreateNode(entry, out var node)) {
                return false;
            }

            var serializedObject = new SerializedObject(asset);
            serializedObject.Update();

            var rootProperty = serializedObject.FindProperty("_root");
            var parentProperty = FindManagedReferenceProperty(rootProperty, parentNode);
            if (parentProperty == null) {
                return false;
            }

            var childProperty = parentProperty.FindPropertyRelative(childFieldName);
            if (childProperty == null) {
                return false;
            }

            Undo.RecordObject(asset, "Assign Ability Graph Child Node");
            childProperty.managedReferenceValue = node;
            serializedObject.ApplyModifiedProperties();

            EditorUtility.SetDirty(asset);
            return true;
        }

        public static bool RemoveNodeReference(AbilityAsset asset, IAbilityStepConfig targetNode) {
            if (asset == null || targetNode == null) {
                return false;
            }

            var serializedObject = new SerializedObject(asset);
            serializedObject.Update();

            var rootProperty = serializedObject.FindProperty("_root");
            if (rootProperty == null) {
                return false;
            }

            if (ReferenceEquals(rootProperty.managedReferenceValue, targetNode)) {
                Undo.RecordObject(asset, "Remove Ability Graph Root Node");
                rootProperty.managedReferenceValue = null;
                serializedObject.ApplyModifiedProperties();
                EditorUtility.SetDirty(asset);
                return true;
            }

            if (!TryFindReferenceToTarget(rootProperty, targetNode, out var referenceProperty, out var listProperty, out var elementIndex)) {
                return false;
            }

            Undo.RecordObject(asset, "Remove Ability Graph Node");
            if (listProperty != null && elementIndex >= 0) {
                listProperty.DeleteArrayElementAtIndex(elementIndex);
                if (elementIndex < listProperty.arraySize) {
                    var candidate = listProperty.GetArrayElementAtIndex(elementIndex);
                    if (candidate.propertyType == SerializedPropertyType.ManagedReference && candidate.managedReferenceValue == null) {
                        listProperty.DeleteArrayElementAtIndex(elementIndex);
                    }
                }
            }
            else {
                referenceProperty.managedReferenceValue = null;
            }

            serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(asset);
            return true;
        }

        public static bool ConnectPort(AbilityAsset asset, IAbilityStepConfig parentConfig, string portLabel, IAbilityStepConfig childConfig) {
            if (asset == null || parentConfig == null || childConfig == null) {
                return false;
            }

            var serializedObject = new SerializedObject(asset);
            serializedObject.Update();

            var rootProperty = serializedObject.FindProperty("_root");
            var parentProperty = FindManagedReferenceProperty(rootProperty, parentConfig);
            if (parentProperty == null) {
                return false;
            }

            if (!TryResolvePortField(parentProperty, portLabel, out var targetProperty, out var arrayIndex)) {
                return false;
            }

            Undo.RecordObject(asset, "Connect Ability Graph Nodes");

            if (arrayIndex >= 0) {
                if (arrayIndex >= targetProperty.arraySize) {
                    return false;
                }

                targetProperty.GetArrayElementAtIndex(arrayIndex).managedReferenceValue = childConfig;
            }
            else {
                targetProperty.managedReferenceValue = childConfig;
            }

            serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(asset);
            return true;
        }

        public static bool DisconnectPort(AbilityAsset asset, IAbilityStepConfig parentConfig, string portLabel) {
            if (asset == null || parentConfig == null) {
                return false;
            }

            var serializedObject = new SerializedObject(asset);
            serializedObject.Update();

            var rootProperty = serializedObject.FindProperty("_root");
            var parentProperty = FindManagedReferenceProperty(rootProperty, parentConfig);
            if (parentProperty == null) {
                return false;
            }

            if (!TryResolvePortField(parentProperty, portLabel, out var targetProperty, out var arrayIndex)) {
                return false;
            }

            Undo.RecordObject(asset, "Disconnect Ability Graph Nodes");

            if (arrayIndex >= 0) {
                if (arrayIndex >= targetProperty.arraySize) {
                    return false;
                }

                targetProperty.GetArrayElementAtIndex(arrayIndex).managedReferenceValue = null;
            }
            else {
                targetProperty.managedReferenceValue = null;
            }

            serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(asset);
            return true;
        }

        private static bool TryResolvePortField(SerializedProperty parentProperty, string portLabel, out SerializedProperty property, out int arrayIndex) {
            property = null;
            arrayIndex = -1;

            string fieldName;
            var index = -1;

            if (portLabel != null && portLabel.StartsWith("Step ") && int.TryParse(portLabel.Substring(5), out var stepIdx)) {
                fieldName = "_children";
                index = stepIdx - 1;
            }
            else if (portLabel != null && portLabel.StartsWith("Branch ") && int.TryParse(portLabel.Substring(7), out var branchIdx)) {
                fieldName = "_children";
                index = branchIdx - 1;
            }
            else if (portLabel == "True") {
                fieldName = "_ifTrue";
            }
            else if (portLabel == "False") {
                fieldName = "_ifFalse";
            }
            else if (portLabel == "Body") {
                fieldName = "_body";
            }
            else {
                return false;
            }

            property = parentProperty.FindPropertyRelative(fieldName);
            if (property == null) {
                return false;
            }

            if (index >= 0 && !property.isArray) {
                return false;
            }

            arrayIndex = index;
            return true;
        }

        public static SerializedProperty FindManagedReferenceProperty(SerializedProperty property, object target) {
            if (property == null || target == null) {
                return null;
            }

            if (property.propertyType == SerializedPropertyType.ManagedReference
                && ReferenceEquals(property.managedReferenceValue, target)) {
                return property.Copy();
            }

            var iterator = property.Copy();
            var end = iterator.GetEndProperty();
            var enterChildren = true;
            while (iterator.NextVisible(enterChildren) && !SerializedProperty.EqualContents(iterator, end)) {
                if (iterator.propertyType == SerializedPropertyType.ManagedReference
                    && ReferenceEquals(iterator.managedReferenceValue, target)) {
                    return iterator.Copy();
                }

                enterChildren = true;
            }

            return null;
        }

        private static bool TryFindReferenceToTarget(
            SerializedProperty property,
            object target,
            out SerializedProperty referenceProperty,
            out SerializedProperty listProperty,
            out int elementIndex) {
            referenceProperty = null;
            listProperty = null;
            elementIndex = -1;

            if (property == null || target == null) {
                return false;
            }

            var iterator = property.Copy();
            var end = iterator.GetEndProperty();
            var enterChildren = true;
            while (iterator.NextVisible(enterChildren) && !SerializedProperty.EqualContents(iterator, end)) {
                if (iterator.propertyType == SerializedPropertyType.ManagedReference
                    && ReferenceEquals(iterator.managedReferenceValue, target)) {
                    referenceProperty = iterator.Copy();
                    if (TryGetManagedReferenceArrayParent(iterator, out var arrayProperty, out var index)) {
                        listProperty = arrayProperty;
                        elementIndex = index;
                    }

                    return true;
                }

                enterChildren = true;
            }

            return false;
        }

        private static bool TryGetManagedReferenceArrayParent(SerializedProperty property, out SerializedProperty arrayProperty, out int index) {
            arrayProperty = null;
            index = -1;

            var propertyPath = property.propertyPath;
            var arrayMarker = ".Array.data[";
            var markerIndex = propertyPath.LastIndexOf(arrayMarker, StringComparison.Ordinal);
            if (markerIndex < 0) {
                return false;
            }

            var indexStart = markerIndex + arrayMarker.Length;
            var indexEnd = propertyPath.IndexOf(']', indexStart);
            if (indexEnd <= indexStart) {
                return false;
            }

            var arrayPath = propertyPath.Substring(0, markerIndex);
            if (!int.TryParse(propertyPath.Substring(indexStart, indexEnd - indexStart), out index)) {
                index = -1;
                return false;
            }

            arrayProperty = property.serializedObject.FindProperty(arrayPath);
            return arrayProperty != null && arrayProperty.isArray;
        }

        private static bool TryCreateNode(AbilityGraphNodeTypeRegistry.Entry entry, out IAbilityStepConfig node) {
            node = null;
            if (entry?.Type == null) {
                return false;
            }

            var instance = Activator.CreateInstance(entry.Type);
            if (instance is not IAbilityStepConfig createdNode) {
                EditorUtility.DisplayDialog("Add Node Failed", $"Failed to create node type '{entry.Type.FullName}'.", "OK");
                return false;
            }

            node = createdNode;
            return true;
        }
    }
}