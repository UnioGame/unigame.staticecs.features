namespace UniGame.StaticEcs.Features.Editor.AbilityGraph
{
    using System;
    using System.Collections.Generic;
    using UnityEditor;
    using UnityEngine;

    internal sealed class AbilityGraphNodePickerWindow : EditorWindow
    {
        private static Action<AbilityGraphNodeTypeRegistry.Entry> _onPick;

        private string _filter = string.Empty;
        private Vector2 _scrollPosition;
        private IReadOnlyList<AbilityGraphNodeTypeRegistry.Entry> _entries;

        public static void Open(Action<AbilityGraphNodeTypeRegistry.Entry> onPick)
        {
            _onPick = onPick;

            var window = CreateInstance<AbilityGraphNodePickerWindow>();
            window.titleContent = new GUIContent("Add Ability Node");
            window.minSize = new Vector2(420f, 480f);
            window.ShowUtility();
            window.Focus();
        }

        private void OnEnable()
        {
            _entries = AbilityGraphNodeTypeRegistry.GetEntries();
        }

        private void OnGUI()
        {
            DrawToolbar();

            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);
            DrawEntries();
            EditorGUILayout.EndScrollView();
        }

        private void DrawToolbar()
        {
            using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
            {
                _filter = GUILayout.TextField(
                    _filter ?? string.Empty,
                    EditorStyles.toolbarSearchField,
                    GUILayout.ExpandWidth(true)
                );

                if (GUILayout.Button("Close", EditorStyles.toolbarButton, GUILayout.Width(60f)))
                    Close();
            }
        }

        private void DrawEntries()
        {
            if (_entries == null || _entries.Count == 0)
            {
                EditorGUILayout.HelpBox(
                    "No creatable ability node types were discovered.",
                    MessageType.Warning
                );
                return;
            }

            string currentCategory = null;
            var shown = 0;
            for (var i = 0; i < _entries.Count; i++)
            {
                var entry = _entries[i];
                if (!MatchesFilter(entry))
                    continue;

                shown++;
                if (
                    !string.Equals(
                        currentCategory,
                        entry.Category,
                        StringComparison.OrdinalIgnoreCase
                    )
                )
                {
                    currentCategory = entry.Category;
                    EditorGUILayout.Space(4f);
                    EditorGUILayout.LabelField(currentCategory, EditorStyles.boldLabel);
                }

                using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
                {
                    EditorGUILayout.LabelField(entry.DisplayName, EditorStyles.boldLabel);
                    EditorGUILayout.LabelField(entry.Type.FullName, EditorStyles.miniLabel);

                    if (GUILayout.Button("Add Node", GUILayout.Width(90f)))
                    {
                        _onPick?.Invoke(entry);
                        Close();
                        GUIUtility.ExitGUI();
                    }
                }
            }

            if (shown == 0)
                EditorGUILayout.HelpBox(
                    "No node types match the current filter.",
                    MessageType.Info
                );
        }

        private bool MatchesFilter(AbilityGraphNodeTypeRegistry.Entry entry)
        {
            if (string.IsNullOrWhiteSpace(_filter))
                return true;

            return entry.SearchText.IndexOf(
                    _filter.Trim().ToLowerInvariant(),
                    StringComparison.Ordinal
                ) >= 0;
        }
    }
}
