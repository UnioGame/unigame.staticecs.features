namespace UniGame.StaticEcs.Features.Editor.AbilityGraph
{
    using System.Collections.Generic;
    using FFS.Libraries.StaticEcs;
    using Unity;
    using UnityEditor;
    using UnityEngine;

    internal sealed class AbilityGraphBrowserWindow : EditorWindow
    {
        private enum GraphSourceMode
        {
            Project,
            Runtime,
        }

        private readonly List<AbilityAsset> _projectAssets = new();
        private readonly AbilityGraphRuntimeBrowser _runtimeBrowser = new();
        private GraphSourceMode _sourceMode = GraphSourceMode.Project;
        private string _projectAssetFilter = string.Empty;
        private string _runtimeFilter = string.Empty;
        private Vector2 _scrollPosition;

        [MenuItem("UniGame/Static ECS/Ability Graphs/Open Project Graph...")]
        public static void OpenProjectBrowser()
        {
            OpenBrowser(GraphSourceMode.Project);
        }

        [MenuItem("UniGame/Static ECS/Ability Graphs/Open Runtime Graph...")]
        public static void OpenRuntimeBrowser()
        {
            OpenBrowser(GraphSourceMode.Runtime);
        }

        private static void OpenBrowser(GraphSourceMode sourceMode)
        {
            var window = GetWindow<AbilityGraphBrowserWindow>();
            window.titleContent = new GUIContent("Ability Graph Browser");
            window._sourceMode = sourceMode;
            window.RefreshData();
            window.Show();
            window.Focus();
        }

        private void OnEnable()
        {
            titleContent = new GUIContent("Ability Graph Browser");
            RefreshData();
        }

        private void OnProjectChange()
        {
            RefreshData();
            Repaint();
        }

        private void OnGUI()
        {
            DrawToolbar();

            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);
            switch (_sourceMode)
            {
                case GraphSourceMode.Project:
                    DrawProjectBrowser();
                    break;
                case GraphSourceMode.Runtime:
                    DrawRuntimeBrowser();
                    break;
            }

            EditorGUILayout.EndScrollView();
        }

        private void DrawToolbar()
        {
            using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
            {
                if (
                    GUILayout.Toggle(
                        _sourceMode == GraphSourceMode.Project,
                        "Project",
                        EditorStyles.toolbarButton
                    )
                )
                {
                    _sourceMode = GraphSourceMode.Project;
                }

                if (
                    GUILayout.Toggle(
                        _sourceMode == GraphSourceMode.Runtime,
                        "Runtime",
                        EditorStyles.toolbarButton
                    )
                )
                {
                    _sourceMode = GraphSourceMode.Runtime;
                }

                GUILayout.FlexibleSpace();

                if (_sourceMode == GraphSourceMode.Project)
                {
                    _projectAssetFilter = GUILayout.TextField(
                        _projectAssetFilter ?? string.Empty,
                        EditorStyles.toolbarSearchField,
                        GUILayout.Width(260f)
                    );
                }
                else
                {
                    _runtimeFilter = GUILayout.TextField(
                        _runtimeFilter ?? string.Empty,
                        EditorStyles.toolbarSearchField,
                        GUILayout.Width(260f)
                    );
                }

                if (GUILayout.Button("Refresh", EditorStyles.toolbarButton, GUILayout.Width(70f)))
                {
                    RefreshData();
                }
            }
        }

        private void DrawProjectBrowser()
        {
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.LabelField(
                    $"Project Ability Assets: {_projectAssets.Count}",
                    EditorStyles.boldLabel
                );
                EditorGUILayout.HelpBox(
                    "Choose a project ability graph to open it in the tabbed graph workspace.",
                    MessageType.Info
                );

                var shown = 0;
                for (var i = 0; i < _projectAssets.Count; i++)
                {
                    var asset = _projectAssets[i];
                    if (asset == null || !MatchesProjectAssetFilter(asset))
                    {
                        continue;
                    }

                    shown++;
                    using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
                    {
                        EditorGUILayout.LabelField(
                            BuildProjectAssetTitle(asset),
                            EditorStyles.boldLabel
                        );

                        var rootType = asset.Root != null ? asset.Root.GetType().Name : "<no root>";
                        EditorGUILayout.LabelField($"Display: {asset.DisplayName}");
                        EditorGUILayout.LabelField($"Root: {rootType}");

                        using (new EditorGUILayout.HorizontalScope())
                        {
                            if (GUILayout.Button("Open Graph", GUILayout.Width(95f)))
                            {
                                AbilityGraphEditorWindow.OpenTab(asset);
                            }

                            if (GUILayout.Button("Ping", GUILayout.Width(70f)))
                            {
                                EditorGUIUtility.PingObject(asset);
                            }

                            if (GUILayout.Button("Select", GUILayout.Width(70f)))
                            {
                                Selection.activeObject = asset;
                            }
                        }
                    }
                }

                if (shown == 0)
                {
                    EditorGUILayout.HelpBox(
                        "No project AbilityAsset matches the current filter.",
                        MessageType.None
                    );
                }
            }
        }

        private void DrawRuntimeBrowser()
        {
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.LabelField("Runtime Ability Browser", EditorStyles.boldLabel);
                EditorGUILayout.HelpBox(
                    "Choose a runtime-backed ability to open its authored graph in the workspace while keeping owner inspection actions available here.",
                    MessageType.Info
                );

                if (World<Main>.Status != WorldStatus.Initialized)
                {
                    EditorGUILayout.HelpBox(
                        "World<Main> is not initialized. Enter Play Mode to browse runtime abilities.",
                        MessageType.Info
                    );
                    return;
                }

                _runtimeBrowser.Refresh();
                DrawRuntimeSection("Active Casts", _runtimeBrowser.ActiveCasts);
                EditorGUILayout.Space(6f);
                DrawRuntimeSection("Equipped Abilities", _runtimeBrowser.EquippedAbilities);
            }
        }

        private void DrawRuntimeSection(
            string title,
            IReadOnlyList<AbilityGraphRuntimeBrowser.RuntimeAbilityEntry> entries
        )
        {
            EditorGUILayout.LabelField($"{title}: {entries.Count}", EditorStyles.boldLabel);

            var shown = 0;
            for (var i = 0; i < entries.Count; i++)
            {
                var entry = entries[i];
                if (!MatchesRuntimeFilter(entry))
                {
                    continue;
                }

                shown++;
                using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
                {
                    EditorGUILayout.LabelField(
                        BuildRuntimeEntryTitle(entry),
                        EditorStyles.boldLabel
                    );

                    var ownerName =
                        entry.OwnerGameObject != null
                            ? entry.OwnerGameObject.name
                            : "<no GameObject>";
                    EditorGUILayout.LabelField($"Owner: {ownerName}  {entry.Owner}");
                    if (entry.IsActiveCast)
                    {
                        EditorGUILayout.LabelField($"Cast: {entry.Cast}");
                        if (!string.IsNullOrWhiteSpace(entry.ActiveNodeGuid))
                        {
                            EditorGUILayout.LabelField($"Active node: {entry.ActiveNodeGuid}");
                        }
                    }

                    using (new EditorGUILayout.HorizontalScope())
                    {
                        using (new EditorGUI.DisabledScope(entry.Asset == null))
                        {
                            if (GUILayout.Button("Open Graph", GUILayout.Width(95f)))
                            {
                                AbilityGraphEditorWindow.OpenTab(entry.Asset);
                            }
                        }

                        using (new EditorGUI.DisabledScope(entry.OwnerGameObject == null))
                        {
                            if (GUILayout.Button("Ping Owner", GUILayout.Width(90f)))
                            {
                                EditorGUIUtility.PingObject(entry.OwnerGameObject);
                            }

                            if (GUILayout.Button("Select Owner", GUILayout.Width(95f)))
                            {
                                Selection.activeGameObject = entry.OwnerGameObject;
                            }
                        }
                    }
                }
            }

            if (shown == 0)
            {
                EditorGUILayout.HelpBox(
                    "No runtime entries match the current filter.",
                    MessageType.None
                );
            }
        }

        private void RefreshData()
        {
            RefreshProjectAssets();
            if (_sourceMode == GraphSourceMode.Runtime)
            {
                _runtimeBrowser.Refresh();
            }
        }

        private void RefreshProjectAssets()
        {
            _projectAssets.Clear();

            var guids = AssetDatabase.FindAssets("t:AbilityAsset");
            for (var i = 0; i < guids.Length; i++)
            {
                var path = AssetDatabase.GUIDToAssetPath(guids[i]);
                var asset = AssetDatabase.LoadAssetAtPath<AbilityAsset>(path);
                if (asset == null)
                {
                    continue;
                }

                _projectAssets.Add(asset);
            }

            _projectAssets.Sort(
                (left, right) =>
                    string.Compare(left.name, right.name, System.StringComparison.OrdinalIgnoreCase)
            );
        }

        private bool MatchesProjectAssetFilter(AbilityAsset asset)
        {
            if (string.IsNullOrWhiteSpace(_projectAssetFilter))
            {
                return true;
            }

            var filter = _projectAssetFilter.Trim();
            var rootType = asset.Root != null ? asset.Root.GetType().Name : string.Empty;
            return ContainsIgnoreCase(asset.name, filter)
                || ContainsIgnoreCase(asset.DisplayName, filter)
                || ContainsIgnoreCase(asset.Id.ToString(), filter)
                || ContainsIgnoreCase(rootType, filter);
        }

        private bool MatchesRuntimeFilter(AbilityGraphRuntimeBrowser.RuntimeAbilityEntry entry)
        {
            if (string.IsNullOrWhiteSpace(_runtimeFilter))
            {
                return true;
            }

            var filter = _runtimeFilter.Trim();
            return ContainsIgnoreCase(entry.DisplayName, filter)
                || ContainsIgnoreCase(entry.AbilityId.ToString(), filter)
                || ContainsIgnoreCase(entry.Owner.ToString(), filter)
                || ContainsIgnoreCase(entry.ActiveNodeGuid, filter)
                || (
                    entry.OwnerGameObject != null
                    && ContainsIgnoreCase(entry.OwnerGameObject.name, filter)
                );
        }

        private static string BuildProjectAssetTitle(AbilityAsset asset)
        {
            return $"{asset.name}  [{asset.Id.Value}]";
        }

        private static string BuildRuntimeEntryTitle(
            AbilityGraphRuntimeBrowser.RuntimeAbilityEntry entry
        )
        {
            var assetName = entry.Asset != null ? entry.Asset.name : "<unresolved asset>";
            return $"{entry.DisplayName}  [{entry.AbilityId.Value}]  Asset: {assetName}";
        }

        private static bool ContainsIgnoreCase(string value, string search)
        {
            return !string.IsNullOrEmpty(value)
                && value.IndexOf(search, System.StringComparison.OrdinalIgnoreCase) >= 0;
        }
    }
}
