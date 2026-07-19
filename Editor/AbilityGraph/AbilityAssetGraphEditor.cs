using UnityEditor;
using UnityEngine;

namespace UniGame.StaticEcs.Features.Editor.AbilityGraph {
    [CustomEditor(typeof(AbilityAsset))]
    public sealed class AbilityAssetGraphEditor : UnityEditor.Editor {
        public override void OnInspectorGUI() {
            DrawDefaultInspector();

            EditorGUILayout.Space();
            EditorGUILayout.HelpBox(
                "Ability graphs are edited from the dedicated editor window. Runtime data still comes from AbilityAsset.Root and the existing SerializeReference config tree.",
                MessageType.Info);

            using (new EditorGUILayout.HorizontalScope()) {
                if (GUILayout.Button("Open Graph Editor")) {
                    AbilityGraphEditorWindow.OpenTab((AbilityAsset)target);
                }

                if (GUILayout.Button("Ping In Project")) {
                    EditorGUIUtility.PingObject(target);
                }
            }
        }
    }
}