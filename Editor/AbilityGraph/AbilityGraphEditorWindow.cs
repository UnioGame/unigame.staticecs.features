using System;
using System.Collections.Generic;
using System.Reflection;
using FFS.Libraries.StaticEcs;
using unigame.staticecs.unity;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace unigame.staticecs.features.Editor.AbilityGraph {
    public sealed class AbilityGraphEditorWindow : EditorWindow {
        private static class OdinNodeInspectorBridge {
            private static readonly Type PropertyTreeType = Type.GetType("Sirenix.OdinInspector.Editor.PropertyTree, Sirenix.OdinInspector.Editor");
            private static readonly MethodInfo CreateMethod = PropertyTreeType?.GetMethod(
                "Create",
                BindingFlags.Public | BindingFlags.Static,
                null,
                new[] { typeof(object) },
                null);
            private static readonly MethodInfo DrawWithChildrenMethod = PropertyTreeType?.GetMethod(
                "Draw",
                BindingFlags.Public | BindingFlags.Instance,
                null,
                new[] { typeof(bool) },
                null);
            private static readonly MethodInfo DrawMethod = DrawWithChildrenMethod == null
                ? PropertyTreeType?.GetMethod("Draw", BindingFlags.Public | BindingFlags.Instance, null, Type.EmptyTypes, null)
                : null;
            private static readonly MethodInfo ApplyChangesMethod = PropertyTreeType?.GetMethod("ApplyChanges", BindingFlags.Public | BindingFlags.Instance);
            private static readonly MethodInfo DisposeMethod = PropertyTreeType?.GetMethod("Dispose", BindingFlags.Public | BindingFlags.Instance);

            public static bool IsAvailable => CreateMethod != null && (DrawWithChildrenMethod != null || DrawMethod != null);

            public static object CreateTree(object target) {
                return target != null && IsAvailable
                    ? CreateMethod.Invoke(null, new[] { target })
                    : null;
            }

            public static void Draw(object propertyTree) {
                if (propertyTree == null) {
                    return;
                }

                if (DrawWithChildrenMethod != null) {
                    DrawWithChildrenMethod.Invoke(propertyTree, new object[] { false });
                    return;
                }

                DrawMethod?.Invoke(propertyTree, null);
            }

            public static void ApplyChanges(object propertyTree) {
                if (propertyTree == null) {
                    return;
                }

                ApplyChangesMethod?.Invoke(propertyTree, null);
            }

            public static void DisposeTree(object propertyTree) {
                if (propertyTree == null) {
                    return;
                }

                DisposeMethod?.Invoke(propertyTree, null);
            }
        }

        private sealed class AbilityGraphTabState {
            public AbilityAsset Asset;
            public AbilityEditorLayer Layer = AbilityEditorLayer.Execution;
            public IAbilityStepConfig SelectedNodeConfig;
            public string SelectedNodeTitle = string.Empty;
            public object OdinPropertyTree;
            public object OdinTarget;
        }

        private enum AbilityEditorLayer {
            Execution,
            LaunchConditions,
        }

        private readonly List<AbilityGraphTabState> _tabs = new();
        private IMGUIContainer _assetInspector;
        private IMGUIContainer _toolbarContainer;
        private AbilityGraphCanvasView _graphView;
        private IMGUIContainer _launchConditionsContainer;
        private Label _inspectorTitle;
        private Label _titleLabel;
        private VisualElement _emptyState;
        private VisualElement _tabBar;
        private int _activeTabIndex = -1;

        [MenuItem("UniGame/Static ECS/Ability Graph")]
        public static void OpenEmpty() {
            var window = GetWindow<AbilityGraphEditorWindow>();
            window.titleContent = new GUIContent("Ability Graph Workspace");
            window.Show();
            window.Focus();
        }

        public static void Open(AbilityAsset asset) {
            OpenTab(asset);
        }

        public static void OpenTab(AbilityAsset asset) {
            if (asset == null) {
                return;
            }

            var window = GetWindow<AbilityGraphEditorWindow>();
            window.titleContent = new GUIContent("Ability Graph Workspace");
            window.OpenOrFocusTab(asset);
            window.Show();
            window.Focus();
        }

        private void OnEnable() {
            titleContent = new GUIContent("Ability Graph Workspace");
            BuildUi();
            Refresh();
        }

        private void OnProjectChange() {
            RemoveInvalidTabs();
            Refresh();
            Repaint();
        }

        private void OnDisable() {
            DisposeTabResources();
        }

        private void OnSelectionChange() {
            if (GetActiveTab() == null) {
                return;
            }

            Repaint();
        }

        private void OpenOrFocusTab(AbilityAsset asset) {
            for (var i = 0; i < _tabs.Count; i++) {
                if (!ReferenceEquals(_tabs[i].Asset, asset)) {
                    continue;
                }

                _activeTabIndex = i;
                Refresh();
                return;
            }

            _tabs.Add(new AbilityGraphTabState {
                Asset = asset,
            });
            _activeTabIndex = _tabs.Count - 1;
            Refresh();
        }

        private void BuildUi() {
            rootVisualElement.Clear();

            var split = new TwoPaneSplitView(0, 760f, TwoPaneSplitViewOrientation.Horizontal);
            split.style.flexGrow = 1f;

            var graphPane = new VisualElement();
            graphPane.style.flexGrow = 1f;
            graphPane.style.paddingLeft = 12f;
            graphPane.style.paddingRight = 12f;
            graphPane.style.paddingTop = 12f;
            graphPane.style.paddingBottom = 12f;

            _titleLabel = new Label();
            _titleLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            _titleLabel.style.fontSize = 15;
            graphPane.Add(_titleLabel);

            _toolbarContainer = new IMGUIContainer(DrawToolbar);
            _toolbarContainer.style.marginTop = 8f;
            graphPane.Add(_toolbarContainer);

            var tabScroll = new ScrollView(ScrollViewMode.Horizontal);
            tabScroll.style.marginTop = 8f;
            tabScroll.horizontalScrollerVisibility = ScrollerVisibility.Auto;
            tabScroll.verticalScrollerVisibility = ScrollerVisibility.Hidden;
            graphPane.Add(tabScroll);

            _tabBar = new VisualElement();
            _tabBar.style.flexDirection = FlexDirection.Row;
            _tabBar.style.flexGrow = 1f;
            tabScroll.Add(_tabBar);

            _emptyState = BuildEmptyState();
            _emptyState.style.marginTop = 8f;
            graphPane.Add(_emptyState);


            _graphView = new AbilityGraphCanvasView(HandleGraphNodeSelected, Refresh);
            _graphView.style.marginTop = 8f;
            graphPane.Add(_graphView);

            _launchConditionsContainer = new IMGUIContainer(DrawLaunchConditionsSurface);
            _launchConditionsContainer.style.marginTop = 8f;
            graphPane.Add(_launchConditionsContainer);

            var inspectorPane = new VisualElement();
            inspectorPane.style.flexGrow = 1f;
            inspectorPane.style.paddingLeft = 8f;
            inspectorPane.style.paddingRight = 8f;
            inspectorPane.style.paddingTop = 8f;
            inspectorPane.style.paddingBottom = 8f;

            _inspectorTitle = new Label("Ability Asset");
            _inspectorTitle.style.unityFontStyleAndWeight = FontStyle.Bold;
            _inspectorTitle.style.marginBottom = 8f;
            inspectorPane.Add(_inspectorTitle);

            _assetInspector = new IMGUIContainer(DrawAssetInspector);
            _assetInspector.style.flexGrow = 1f;
            inspectorPane.Add(_assetInspector);

            split.Add(graphPane);
            split.Add(inspectorPane);

            rootVisualElement.Add(split);
        }

        private void Refresh() {
            RemoveInvalidTabs();
            if (_titleLabel == null) {
                return;
            }

            RefreshTabBar();

            var activeTab = GetActiveTab();

            if (activeTab == null) {
                _titleLabel.text = "Ability Graph Workspace";
                if (_emptyState != null) {
                    _emptyState.style.display = DisplayStyle.Flex;
                }
                _graphView?.Render(null, AbilityGraphProjection.Empty);
                RefreshLayerVisibility();
                RefreshInspectorTitle();
                _toolbarContainer?.MarkDirtyRepaint();
                _assetInspector?.MarkDirtyRepaint();
                return;
            }

            if (_emptyState != null) {
                _emptyState.style.display = DisplayStyle.None;
            }

            _titleLabel.text = activeTab.Asset.name;
            _graphView?.Render(activeTab.Asset, AbilityGraphProjection.Build(activeTab.Asset));
            RefreshLayerVisibility();
            RefreshInspectorTitle();
            _toolbarContainer?.MarkDirtyRepaint();
            _assetInspector?.MarkDirtyRepaint();
        }

        private void SetEditorLayer(AbilityEditorLayer layer) {
            var activeTab = GetActiveTab();
            if (activeTab == null || activeTab.Layer == layer) {
                return;
            }

            activeTab.Layer = layer;
            if (activeTab.Layer != AbilityEditorLayer.Execution) {
                ResetSelectedNode(activeTab);
                activeTab.SelectedNodeConfig = null;
                activeTab.SelectedNodeTitle = string.Empty;
            }
            Refresh();
        }

        private void HandleGraphNodeSelected(AbilityGraphProjection.Node node) {
            var activeTab = GetActiveTab();
            if (activeTab == null) {
                return;
            }

            if (!ReferenceEquals(activeTab.SelectedNodeConfig, node?.Config)) {
                ResetSelectedNode(activeTab);
            }

            activeTab.SelectedNodeConfig = node?.Config;
            activeTab.SelectedNodeTitle = node?.Title ?? string.Empty;
            RefreshInspectorTitle();
            _assetInspector?.MarkDirtyRepaint();
        }

        private void RefreshLayerVisibility() {
            var activeTab = GetActiveTab();
            var hasTab = activeTab != null;
            var isExecution = hasTab && activeTab.Layer == AbilityEditorLayer.Execution;

            if (_graphView != null) {
                _graphView.style.display = isExecution
                    ? DisplayStyle.Flex
                    : DisplayStyle.None;
            }

            if (_launchConditionsContainer != null) {
                _launchConditionsContainer.style.display = hasTab && !isExecution
                    ? DisplayStyle.Flex
                    : DisplayStyle.None;
            }
        }

        private void RefreshInspectorTitle() {
            if (_inspectorTitle == null) {
                return;
            }

            var activeTab = GetActiveTab();
            if (activeTab == null) {
                _inspectorTitle.text = "Ability Graph Workspace";
                return;
            }

            if (activeTab.Layer == AbilityEditorLayer.Execution && activeTab.SelectedNodeConfig != null) {
                _inspectorTitle.text = $"Node: {activeTab.SelectedNodeTitle}";
                return;
            }

            _inspectorTitle.text = "Ability Asset";
        }

        private void OpenSelectedAbility() {
            if (Selection.activeObject is AbilityAsset asset) {
                OpenOrFocusTab(asset);
            }
        }

        private void PingAsset() {
            var activeTab = GetActiveTab();
            if (activeTab == null) {
                return;
            }

            EditorGUIUtility.PingObject(activeTab.Asset);
            Selection.activeObject = activeTab.Asset;
        }

        private void CloseActiveTab() {
            if (_activeTabIndex < 0 || _activeTabIndex >= _tabs.Count) {
                return;
            }

            DisposeTabResources(_tabs[_activeTabIndex]);
            _tabs.RemoveAt(_activeTabIndex);
            if (_tabs.Count == 0) {
                _activeTabIndex = -1;
            }
            else if (_activeTabIndex >= _tabs.Count) {
                _activeTabIndex = _tabs.Count - 1;
            }

            Refresh();
        }

        private void DrawAssetInspector() {
            var activeTab = GetActiveTab();
            if (activeTab == null) {
                EditorGUILayout.HelpBox("Open a graph tab to inspect the selected ability or node here.", MessageType.Info);
                return;
            }

            if (activeTab.Layer == AbilityEditorLayer.Execution && activeTab.SelectedNodeConfig != null && activeTab.Asset != null) {
                DrawSelectedNodeInspector();
                return;
            }

            var editor = UnityEditor.Editor.CreateEditor(activeTab.Asset);
            if (editor == null) {
                EditorGUILayout.HelpBox("Failed to create AbilityAsset editor.", MessageType.Warning);
                return;
            }

            editor.OnInspectorGUI();
            DestroyImmediate(editor);
        }

        private void DrawSelectedNodeInspector() {
            var activeTab = GetActiveTab();
            if (activeTab == null) {
                return;
            }

            var serializedObject = new SerializedObject(activeTab.Asset);
            serializedObject.Update();

            var rootProperty = serializedObject.FindProperty("_root");
            var nodeProperty = FindManagedReferenceProperty(rootProperty, activeTab.SelectedNodeConfig);
            if (nodeProperty == null) {
                EditorGUILayout.HelpBox("Failed to resolve the selected node inside AbilityAsset.Root.", MessageType.Warning);
                return;
            }

            EditorGUILayout.HelpBox("Editing the selected execution node. Launch conditions and business rules stay outside the execution graph.", MessageType.Info);
            if (TryDrawNodeInspectorWithOdin(activeTab, nodeProperty)) {
                return;
            }

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(nodeProperty, includeChildren: true);
            if (EditorGUI.EndChangeCheck()) {
                serializedObject.ApplyModifiedProperties();
                Refresh();
            }
        }

        private bool TryDrawNodeInspectorWithOdin(AbilityGraphTabState activeTab, SerializedProperty nodeProperty) {
            if (!OdinNodeInspectorBridge.IsAvailable) {
                return false;
            }

            var nodeTarget = nodeProperty.managedReferenceValue;
            if (nodeTarget == null) {
                return false;
            }

            if (!ReferenceEquals(activeTab.OdinTarget, nodeTarget)) {
                ResetSelectedNode(activeTab);
                activeTab.OdinTarget = nodeTarget;
                activeTab.OdinPropertyTree = OdinNodeInspectorBridge.CreateTree(nodeTarget);
            }

            if (activeTab.OdinPropertyTree == null) {
                return false;
            }

            EditorGUI.BeginChangeCheck();
            OdinNodeInspectorBridge.Draw(activeTab.OdinPropertyTree);
            var changed = EditorGUI.EndChangeCheck();
            OdinNodeInspectorBridge.ApplyChanges(activeTab.OdinPropertyTree);
            if (!changed) {
                return true;
            }

            EditorUtility.SetDirty(activeTab.Asset);
            Refresh();
            return true;
        }

        private static SerializedProperty FindManagedReferenceProperty(SerializedProperty property, object target) {
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

        private void DrawLaunchConditionsSurface() {
            var activeTab = GetActiveTab();
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox)) {
                EditorGUILayout.LabelField("Launch Conditions", EditorStyles.boldLabel);
                EditorGUILayout.HelpBox(
                    "Execution graphs and launch conditions are intentionally separate authoring layers. AbilityAsset.Root remains the execution source of truth; launch-condition configs are edited through a different surface.",
                    MessageType.Info);

                if (activeTab == null) {
                    EditorGUILayout.HelpBox("Select an AbilityAsset to inspect the paired execution source while editing the separate condition layer.", MessageType.None);
                    return;
                }

                EditorGUILayout.LabelField("Execution Asset", activeTab.Asset.name);
                EditorGUILayout.LabelField("Execution Root", activeTab.Asset.Root != null ? activeTab.Asset.Root.GetType().Name : "<no root>");
                EditorGUILayout.HelpBox("Condition-layer authoring is not stored in AbilityAsset.Root and will be connected here in the next slice.", MessageType.None);
            }
        }

        private AbilityGraphTabState GetActiveTab() {
            if (_activeTabIndex < 0 || _activeTabIndex >= _tabs.Count) {
                return null;
            }

            return _tabs[_activeTabIndex];
        }

        private void RemoveInvalidTabs() {
            for (var i = _tabs.Count - 1; i >= 0; i--) {
                if (_tabs[i].Asset != null) {
                    continue;
                }

                DisposeTabResources(_tabs[i]);
                _tabs.RemoveAt(i);
            }

            if (_tabs.Count == 0) {
                _activeTabIndex = -1;
                return;
            }

            if (_activeTabIndex >= _tabs.Count) {
                _activeTabIndex = _tabs.Count - 1;
            }
        }

        private void RefreshTabBar() {
            if (_tabBar == null) {
                return;
            }

            _tabBar.Clear();

            for (var i = 0; i < _tabs.Count; i++) {
                var tab = _tabs[i];
                var tabIndex = i;

                var tabRoot = new VisualElement();
                tabRoot.style.flexDirection = FlexDirection.Row;
                tabRoot.style.marginRight = 6f;
                tabRoot.style.alignItems = Align.Center;
                tabRoot.style.backgroundColor = i == _activeTabIndex
                    ? new Color(0.22f, 0.22f, 0.22f, 1f)
                    : new Color(0.14f, 0.14f, 0.14f, 1f);
                tabRoot.style.borderTopLeftRadius = 4f;
                tabRoot.style.borderTopRightRadius = 4f;
                tabRoot.style.borderBottomLeftRadius = 4f;
                tabRoot.style.borderBottomRightRadius = 4f;
                tabRoot.style.paddingLeft = 6f;
                tabRoot.style.paddingRight = 4f;
                tabRoot.style.paddingTop = 3f;
                tabRoot.style.paddingBottom = 3f;

                var tabButton = new Button(() => ActivateTab(tabIndex)) {
                    text = BuildTabTitle(tab),
                };
                tabButton.style.marginRight = 4f;
                tabButton.style.unityTextAlign = TextAnchor.MiddleLeft;
                tabRoot.Add(tabButton);

                var closeButton = new Button(() => CloseTab(tabIndex)) {
                    text = "x",
                };
                closeButton.style.width = 24f;
                closeButton.style.minWidth = 24f;
                tabRoot.Add(closeButton);

                _tabBar.Add(tabRoot);
            }
        }

        private void ActivateTab(int index) {
            if (index < 0 || index >= _tabs.Count || _activeTabIndex == index) {
                return;
            }

            _activeTabIndex = index;
            Refresh();
        }

        private void CloseTab(int index) {
            if (index < 0 || index >= _tabs.Count) {
                return;
            }

            DisposeTabResources(_tabs[index]);
            _tabs.RemoveAt(index);
            if (_tabs.Count == 0) {
                _activeTabIndex = -1;
            }
            else if (_activeTabIndex > index) {
                _activeTabIndex--;
            }
            else if (_activeTabIndex >= _tabs.Count) {
                _activeTabIndex = _tabs.Count - 1;
            }

            Refresh();
        }

        private VisualElement BuildEmptyState() {
            var container = new VisualElement();
            container.style.flexGrow = 1f;
            container.style.justifyContent = Justify.Center;
            container.style.alignItems = Align.Center;
            container.style.minHeight = 320f;

            var title = new Label("No graph tab is open");
            title.style.unityFontStyleAndWeight = FontStyle.Bold;
            title.style.fontSize = 16;
            container.Add(title);

            var description = new Label("Open a project or runtime ability graph from the dedicated browser window. The graph workspace stays focused on tabs and canvas only.");
            description.style.marginTop = 8f;
            description.style.whiteSpace = WhiteSpace.Normal;
            description.style.maxWidth = 520f;
            container.Add(description);

            var buttons = new VisualElement();
            buttons.style.flexDirection = FlexDirection.Row;
            buttons.style.marginTop = 12f;

            var projectButton = new Button(AbilityGraphBrowserWindow.OpenProjectBrowser) {
                text = "Open Project Browser"
            };
            projectButton.style.marginRight = 8f;
            buttons.Add(projectButton);

            var runtimeButton = new Button(AbilityGraphBrowserWindow.OpenRuntimeBrowser) {
                text = "Open Runtime Browser"
            };
            buttons.Add(runtimeButton);
            container.Add(buttons);

            return container;
        }

        private static string BuildTabTitle(AbilityGraphTabState tab) {
            if (tab.Asset == null) {
                return "<missing graph>";
            }

            return string.IsNullOrWhiteSpace(tab.Asset.DisplayName)
                ? tab.Asset.name
                : tab.Asset.DisplayName;
        }

        private void DisposeTabResources() {
            for (var i = 0; i < _tabs.Count; i++) {
                DisposeTabResources(_tabs[i]);
            }
        }

        private static void DisposeTabResources(AbilityGraphTabState tab) {
            if (tab == null) {
                return;
            }

            ResetSelectedNode(tab);
        }

        private static void ResetSelectedNode(AbilityGraphTabState tab) {
            if (tab == null) {
                return;
            }

            OdinNodeInspectorBridge.DisposeTree(tab.OdinPropertyTree);
            tab.OdinPropertyTree = null;
            tab.OdinTarget = null;
        }

        private void DrawToolbar() {
            var activeTab = GetActiveTab();

            using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar)) {
                if (GUILayout.Button("Project", EditorStyles.toolbarButton, GUILayout.Width(64f))) {
                    AbilityGraphBrowserWindow.OpenProjectBrowser();
                }

                if (GUILayout.Button("Runtime", EditorStyles.toolbarButton, GUILayout.Width(64f))) {
                    AbilityGraphBrowserWindow.OpenRuntimeBrowser();
                }

                if (GUILayout.Button("Open Selected", EditorStyles.toolbarButton, GUILayout.Width(92f))) {
                    OpenSelectedAbility();
                }

                using (new EditorGUI.DisabledScope(activeTab == null)) {
                    if (GUILayout.Button("Ping", EditorStyles.toolbarButton, GUILayout.Width(48f))) {
                        PingAsset();
                    }

                    if (GUILayout.Button("Close", EditorStyles.toolbarButton, GUILayout.Width(52f))) {
                        CloseActiveTab();
                    }
                }

                GUILayout.FlexibleSpace();

                var executionSelected = activeTab != null && activeTab.Layer == AbilityEditorLayer.Execution;
                if (GUILayout.Toggle(executionSelected, "Execution", EditorStyles.toolbarButton, GUILayout.Width(78f)) && !executionSelected) {
                    SetEditorLayer(AbilityEditorLayer.Execution);
                }

                var launchSelected = activeTab != null && activeTab.Layer == AbilityEditorLayer.LaunchConditions;
                if (GUILayout.Toggle(launchSelected, "Launch", EditorStyles.toolbarButton, GUILayout.Width(68f)) && !launchSelected) {
                    SetEditorLayer(AbilityEditorLayer.LaunchConditions);
                }
            }
        }
    }
}