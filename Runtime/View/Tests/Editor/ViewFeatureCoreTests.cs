namespace UniGame.StaticEcs.Features.Tests
{
    using System;
    using FFS.Libraries.StaticEcs;
    using NUnit.Framework;
    using UniGame.StaticEcs.Unity;
    using UniGame.UI.Common.Views;
    using UniGame.UiSystem.Runtime;
    using UniModules.UniGame.UiSystem.Runtime;
    using UnityEditor;
    using UnityEngine;

    [TestFixture]
    public sealed class ViewFeatureCoreTests
    {
        private const string TempFolder = "Assets/__ViewFeatureConverterTests";
        private const string PrefabPath = TempFolder + "/EmbeddedView.prefab";

        [TearDown]
        public void TearDown()
        {
            AssetDatabase.DeleteAsset(TempFolder);
            if (World<ViewTestWorld>.Status != WorldStatus.NotCreated)
                World<ViewTestWorld>.Destroy();
        }

        [Test]
        public void Placement_RejectsNoneLayout()
        {
            Assert.Throws<ArgumentOutOfRangeException>(
                () => ViewPlacement.InLayout(ViewType.None));
        }

        [Test]
        public void OpenWithoutReceiver_ReturnsFalse()
        {
            CreateWorld();
            var request = Request();

            Assert.IsFalse(
                ViewOperations.TryOpenNew<ViewTestWorld>(request, out var key));
            Assert.IsFalse(key.IsValid);
        }

        [Test]
        public void OpenOn_PreservesEntityAndNestedPlacement()
        {
            CreateWorld();
            var receiver =
                World<ViewTestWorld>.RegisterEventReceiver<OpenViewOnEntityEvent>();
            try
            {
                var entity = World<ViewTestWorld>.NewEntity<Default>();
                var request = Request();
                request.Placement = ViewPlacement.InContainer("Hud", true);

                Assert.IsTrue(
                    ViewOperations.TryOpenOn<ViewTestWorld>(
                        entity,
                        request,
                        out var key));

                foreach (var received in receiver)
                {
                    Assert.AreEqual(key, received.Value.Key);
                    Assert.AreEqual(entity.GID, received.Value.Entity);
                    Assert.AreEqual("Hud", received.Value.Request.Placement.Container);
                    Assert.IsTrue(received.Value.Request.Placement.UseBusyContainer);
                    return;
                }

                Assert.Fail("Open request was not received.");
            }
            finally
            {
                World<ViewTestWorld>.DeleteEventReceiver(ref receiver);
            }
        }

        [Test]
        public void TypedRequest_UsesViewNameOrExplicitViewId()
        {
            var byType = ViewRequest.For<EmptyView, TestViewModel>();
            var byId = ViewRequest.For<EmptyView, TestViewModel>("inventory-screen");

            Assert.AreEqual(nameof(EmptyView), byType.ViewId);
            Assert.AreEqual("inventory-screen", byId.ViewId);
            Assert.AreEqual(
                typeof(TestViewModel).AssemblyQualifiedName,
                byType.ExpectedViewModelType);
            Assert.AreEqual(
                typeof(EmptyView).AssemblyQualifiedName,
                byType.ExpectedViewType);
            Assert.AreEqual("Window", byType.Placement.Layout);
            Assert.IsTrue(byType.ShowOnOpen);
            Assert.IsTrue(byType.MatchesExpectedViewModel(typeof(TestViewModel)));
            Assert.IsFalse(byType.MatchesExpectedViewModel(typeof(OtherTestViewModel)));
            Assert.IsTrue(byType.MatchesExpectedView(typeof(EmptyView)));
            Assert.IsFalse(byType.MatchesExpectedView(typeof(OtherTestView)));
        }

        [Test]
        public void RequestDistinguishesUnspecifiedOrderFromExplicitZero()
        {
            var request = ViewRequest.For<EmptyView>();
            Assert.IsFalse(request.HasOrder);
            Assert.AreEqual(0, request.Order);

            request.Order = 0;
            Assert.IsTrue(request.HasOrder);
            var typed = ViewRequest.Bind<EmptyView, TestViewModel>(request);
            Assert.IsTrue(typed.HasOrder);
            Assert.AreEqual(0, typed.Order);

            var restored = JsonUtility.FromJson<ViewRequest>(JsonUtility.ToJson(typed));
            Assert.IsTrue(restored.HasOrder);
            Assert.AreEqual(0, restored.Order);
        }

        [Test]
        public void ViewOnlyRequest_UsesViewClassAndConfiguredModel()
        {
            var request = ViewRequest.For<EmptyView>();

            Assert.AreEqual(nameof(EmptyView), request.ViewId);
            Assert.AreEqual(typeof(EmptyView).AssemblyQualifiedName, request.ExpectedViewType);
            Assert.IsTrue(string.IsNullOrEmpty(request.ExpectedViewModelType));
            Assert.AreEqual("Window", request.Placement.Layout);
            Assert.IsTrue(request.ShowOnOpen);
        }

        [Test]
        public void OpenByType_PreservesOptionsAndReplacesViewId()
        {
            CreateWorld();
            var newReceiver = World<ViewTestWorld>.RegisterEventReceiver<OpenViewEvent>();
            var onReceiver = World<ViewTestWorld>.RegisterEventReceiver<OpenViewOnEntityEvent>();
            var owner = World<ViewTestWorld>.NewEntity<Default>();
            var target = World<ViewTestWorld>.NewEntity<Default>();
            var request = new ViewRequest
            {
                ViewId = "ignored.alias",
                Skin = "dark",
                Queue = "dialogs",
                Owner = owner.GID,
                Source = owner.GID,
                Placement = ViewPlacement.InLayout("CustomWindow"),
                Order = 7,
                ShowOnOpen = true,
                ExpectedViewModelType = typeof(OtherTestViewModel).AssemblyQualifiedName
            };

            try
            {
                Assert.IsTrue(ViewOperations.TryOpenNewByType<ViewTestWorld, EmptyView>(
                    request, out var newKey));
                Assert.IsTrue(newKey.IsValid);
                Assert.IsTrue(ViewOperations.TryOpenOnByType<ViewTestWorld, EmptyView>(
                    target, request, out var onKey));
                Assert.IsTrue(onKey.IsValid);

                var newCount = 0;
                foreach (var received in newReceiver)
                {
                    newCount++;
                    AssertTypeRequest(received.Value.Request, request);
                }

                var onCount = 0;
                foreach (var received in onReceiver)
                {
                    onCount++;
                    Assert.AreEqual(target.GID, received.Value.Entity);
                    AssertTypeRequest(received.Value.Request, request);
                }

                Assert.AreEqual(1, newCount);
                Assert.AreEqual(1, onCount);
            }
            finally
            {
                World<ViewTestWorld>.DeleteEventReceiver(ref newReceiver);
                World<ViewTestWorld>.DeleteEventReceiver(ref onReceiver);
            }
        }

        [Test]
        public void OpenByType_WithoutOptionsShowsInWindow()
        {
            CreateWorld();
            var receiver = World<ViewTestWorld>.RegisterEventReceiver<OpenViewEvent>();
            try
            {
                Assert.IsTrue(ViewOperations.TryOpenNewByType<ViewTestWorld, EmptyView>(
                    out var key));
                Assert.IsTrue(key.IsValid);
                foreach (var received in receiver)
                {
                    Assert.AreEqual(nameof(EmptyView), received.Value.Request.ViewId);
                    Assert.AreEqual("Window", received.Value.Request.Placement.Layout);
                    Assert.IsTrue(received.Value.Request.ShowOnOpen);
                    return;
                }

                Assert.Fail("No typed open request was sent.");
            }
            finally
            {
                World<ViewTestWorld>.DeleteEventReceiver(ref receiver);
            }
        }

        private static void AssertTypeRequest(ViewRequest actual, ViewRequest options)
        {
            Assert.AreEqual(nameof(EmptyView), actual.ViewId);
            Assert.AreEqual(typeof(EmptyView).AssemblyQualifiedName, actual.ExpectedViewType);
            Assert.IsTrue(string.IsNullOrEmpty(actual.ExpectedViewModelType));
            Assert.AreEqual(options.Skin, actual.Skin);
            Assert.AreEqual(options.Queue, actual.Queue);
            Assert.AreEqual(options.Owner, actual.Owner);
            Assert.AreEqual(options.Source, actual.Source);
            Assert.AreEqual(options.Placement.Layout, actual.Placement.Layout);
            Assert.AreEqual(options.Order, actual.Order);
            Assert.AreEqual(options.ShowOnOpen, actual.ShowOnOpen);
        }

        [Test]
        public void ViewOperations_EnumUsesExactConfiguredId()
        {
            CreateWorld();
            var receiver = World<ViewTestWorld>.RegisterEventReceiver<OpenViewOnEntityEvent>();
            var entity = World<ViewTestWorld>.NewEntity<Default>();
            try
            {
                var request = Request();
                request.ViewId = "IgnoredValue";
                Assert.IsTrue(ViewSystemIdResolver.TryResolve(
                    TestViewId.InventoryAlias,
                    out var resolvedId));
                Assert.AreEqual("inventory.screen", resolvedId);
                request.ViewId = resolvedId;
                Assert.IsTrue(ViewOperations.TryOpenOn<ViewTestWorld>(
                    entity,
                    request,
                    out _));

                entity.Delete<ViewComponent>();
                request.ViewId = "IgnoredValue";
                Assert.IsTrue(ViewOperations.TryOpenOn<ViewTestWorld, TestViewId>(
                    entity,
                    TestViewId.InventoryAlias,
                    request,
                    out var key));
                Assert.IsTrue(key.IsValid);

                var foundMappedRequest = false;
                foreach (var received in receiver)
                {
                    if (received.Value.Request.ViewId == "inventory.screen" &&
                        received.Value.Entity == entity.GID)
                        foundMappedRequest = true;
                }

                Assert.IsTrue(foundMappedRequest, "Mapped open request was not received.");
            }
            finally
            {
                World<ViewTestWorld>.DeleteEventReceiver(ref receiver);
            }
        }

        [Test]
        public void ViewOperations_EnumOpensNewViewWithExactConfiguredId()
        {
            CreateWorld();
            var receiver = World<ViewTestWorld>.RegisterEventReceiver<OpenViewEvent>();
            try
            {
                var request = Request();
                request.ViewId = "IgnoredValue";

                Assert.IsTrue(ViewOperations.TryOpenNew<ViewTestWorld, TestViewId>(
                    TestViewId.InventoryAlias,
                    request,
                    out var key));
                Assert.IsTrue(key.IsValid);

                var foundMappedRequest = false;
                foreach (var received in receiver)
                {
                    if (received.Value.Request.ViewId == "inventory.screen")
                        foundMappedRequest = true;
                }

                Assert.IsTrue(foundMappedRequest, "Mapped open request was not received.");
            }
            finally
            {
                World<ViewTestWorld>.DeleteEventReceiver(ref receiver);
            }
        }

        [Test]
        public void ViewOperations_EnumWithoutMappingFailsWithoutSending()
        {
            CreateWorld();
            var receiver = World<ViewTestWorld>.RegisterEventReceiver<OpenViewEvent>();
            try
            {
                Assert.IsFalse(ViewOperations.TryOpenNew(
                    TestViewId.Unmapped,
                    Request(),
                    out var key));
                Assert.IsFalse(key.IsValid);

                foreach (var _ in receiver)
                    Assert.Fail("An unmapped enum value must not send an open request.");
            }
            finally
            {
                World<ViewTestWorld>.DeleteEventReceiver(ref receiver);
            }
        }

        [Test]
        public void EnumViewSystemIdAttribute_IsVisibleOnMember()
        {
            var field = typeof(TestViewId).GetField(
                nameof(TestViewId.InventoryAlias),
                System.Reflection.BindingFlags.Public |
                System.Reflection.BindingFlags.Static);

            Assert.IsNotNull(field);
            Assert.IsNotNull(Attribute.GetCustomAttribute(
                field,
                typeof(ViewSystemIdAttribute)));
        }

        [Test]
        public void ShowScreenSystem_ConsumesRequestAndSendsTypedViewRequestOnce()
        {
            CreateWorld();
            var receiver = World<ViewTestWorld>.RegisterEventReceiver<OpenViewEvent>();
            var source = World<ViewTestWorld>.NewEntity<Default>();
            source.Set(new OpenTestScreenComponent());
            var request = new ViewRequest
            {
                Skin = "dark",
                Placement = ViewPlacement.InLayout("InventoryOverlay"),
                ShowOnOpen = true
            };

            try
            {
                var system = new ShowScreenSystem<
                    ViewTestWorld,
                    OpenTestScreenComponent,
                    EmptyView,
                    TestViewModel>(request);

                system.Update();
                system.Update();

                Assert.IsFalse(source.Has<OpenTestScreenComponent>());
                var receivedCount = 0;
                foreach (var received in receiver)
                {
                    receivedCount++;
                    Assert.AreEqual(nameof(EmptyView), received.Value.Request.ViewId);
                    Assert.AreEqual(
                        typeof(TestViewModel).AssemblyQualifiedName,
                        received.Value.Request.ExpectedViewModelType);
                    Assert.AreEqual(
                        typeof(EmptyView).AssemblyQualifiedName,
                        received.Value.Request.ExpectedViewType);
                    Assert.AreEqual(source.GID, received.Value.Request.Owner);
                    Assert.AreEqual(source.GID, received.Value.Request.Source);
                    Assert.AreEqual("dark", received.Value.Request.Skin);
                    Assert.AreEqual("InventoryOverlay", received.Value.Request.Placement.Layout);
                    Assert.IsTrue(received.Value.Request.ShowOnOpen);
                }

                Assert.AreEqual(1, receivedCount);
            }
            finally
            {
                World<ViewTestWorld>.DeleteEventReceiver(ref receiver);
            }
        }

        [Test]
        public void CloseAll_RemainsAvailableUntilEveryReceiverReadsIt()
        {
            CreateWorld();
            var lifecycleReceiver =
                World<ViewTestWorld>.RegisterEventReceiver<CloseViewsEvent>();
            var gameplayReceiver =
                World<ViewTestWorld>.RegisterEventReceiver<CloseViewsEvent>();
            try
            {
                var filter = new ViewCloseFilter
                {
                    ViewId = "Notification",
                    Layout = "Overlay"
                };

                Assert.IsTrue(ViewOperations.CloseAll<ViewTestWorld>(filter));

                foreach (var received in lifecycleReceiver)
                {
                    Assert.AreEqual(filter.ViewId, received.Value.Filter.ViewId);
                    Assert.AreEqual(filter.Layout, received.Value.Filter.Layout);
                }

                var gameplayReceived = false;
                foreach (var received in gameplayReceiver)
                {
                    gameplayReceived = true;
                    Assert.AreEqual(filter.ViewId, received.Value.Filter.ViewId);
                    Assert.AreEqual(filter.Layout, received.Value.Filter.Layout);
                }

                Assert.IsTrue(
                    gameplayReceived,
                    "The first receiver must not remove the event before other receivers read it.");
            }
            finally
            {
                World<ViewTestWorld>.DeleteEventReceiver(ref lifecycleReceiver);
                World<ViewTestWorld>.DeleteEventReceiver(ref gameplayReceiver);
            }
        }

        [Test]
        public void ClosedGenericModelComponent_IsRegisteredFromCatalog()
        {
            World<ViewTestWorld>.Create();
            var types = World<ViewTestWorld>.Types();
            types.RegisterAll(typeof(ViewComponent).Assembly);
            ViewModelTypeRegistration.RegisterComponents<ViewTestWorld>(
                types,
                new[] { typeof(TestViewModel) });
            World<ViewTestWorld>.Initialize();

            var entity = World<ViewTestWorld>.NewEntity<Default>();
            var model = new TestViewModel();
            Assert.DoesNotThrow(() =>
                entity.Set(new ViewModelComponent<TestViewModel> { Model = model }));
            Assert.AreSame(
                model,
                entity.Read<ViewModelComponent<TestViewModel>>().Model);
            model.Dispose();
        }

        [Test]
        public void TypedModelQuery_ResolvesModelAndSourceRelation()
        {
            World<ViewTestWorld>.Create();
            var types = World<ViewTestWorld>.Types();
            types.RegisterAll(typeof(ViewComponent).Assembly);
            ViewModelTypeRegistration.RegisterComponents<ViewTestWorld>(
                types,
                new[] { typeof(TestViewModel) });
            World<ViewTestWorld>.Initialize();

            var source = World<ViewTestWorld>.NewEntity<Default>();
            var view = World<ViewTestWorld>.NewEntity<Default>();
            var model = new TestViewModel();
            view.Set(new ViewModelComponent<TestViewModel> { Model = model });
            view.Set(new World<ViewTestWorld>.Link<ViewSource>(source.GID));

            var count = 0;
            var filter = default(All<ViewModelComponent<TestViewModel>,
                World<ViewTestWorld>.Link<ViewSource>>);

            foreach (var viewEntity in World<ViewTestWorld>.Query(filter).Entities())
            {
                count++;
                Assert.AreSame(
                    model,
                    viewEntity.Read<ViewModelComponent<TestViewModel>>().Model);
                Assert.AreEqual(
                    source.GID,
                    viewEntity
                        .Read<World<ViewTestWorld>.Link<ViewSource>>()
                        .Value);
            }

            Assert.AreEqual(1, count);
            model.Dispose();
        }

        [Test]
        public void SerializableConverter_BindsAndClosesWithoutRuntimeState()
        {
            CreateWorld();
            var bindReceiver =
                World<ViewTestWorld>.RegisterEventReceiver<BindExistingViewEvent>();
            var closeReceiver =
                World<ViewTestWorld>.RegisterEventReceiver<CloseViewEvent>();
            var host = new GameObject("embedded-view");
            try
            {
                var view = host.AddComponent<EmptyView>();
                Assert.IsNotNull(view);
                var entity = World<ViewTestWorld>.NewEntity<Default>();
                var converter = new EcsEntityViewConverter<ViewTestWorld>
                {
                    view = view,
                    viewId = "EmbeddedView"
                };

                converter.Apply(entity, host);

                var key = ViewKey.Invalid;
                foreach (var received in bindReceiver)
                {
                    key = received.Value.Key;
                    Assert.AreEqual(entity.GID, received.Value.Entity);
                    Assert.AreSame(view, received.Value.View);
                }

                Assert.IsTrue(key.IsValid);
                entity.Set(new ViewComponent { Key = key });
                converter.OnEntityDestroyed(entity, host);

                var closeCount = 0;
                foreach (var received in closeReceiver)
                {
                    closeCount++;
                    Assert.AreEqual(key, received.Value.Key);
                }

                Assert.AreEqual(1, closeCount);
                Assert.IsTrue(converter.IsEnabled);
                Assert.IsTrue(
                    Attribute.IsDefined(
                        typeof(EcsEntityViewConverter<ViewTestWorld>),
                        typeof(SerializableAttribute)));
            }
            finally
            {
                World<ViewTestWorld>.DeleteEventReceiver(ref bindReceiver);
                World<ViewTestWorld>.DeleteEventReceiver(ref closeReceiver);
                UnityEngine.Object.DestroyImmediate(host);
            }
        }

        [Test]
        public void SerializableConverter_RoundTripsThroughProviderPrefab()
        {
            AssetDatabase.CreateFolder("Assets", "__ViewFeatureConverterTests");
            var source = new GameObject("embedded-view-provider");
            var provider = source.AddComponent<StaticEcsEntityProvider>();
            var view = source.AddComponent<EmptyView>();
            Assert.IsNotNull(view);
            provider.serializableConverters.Add(
                new EcsEntityViewConverter
                {
                    view = view,
                    viewId = "EmbeddedView"
                });

            PrefabUtility.SaveAsPrefabAsset(source, PrefabPath);
            UnityEngine.Object.DestroyImmediate(source);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
            var restored = prefab.GetComponent<StaticEcsEntityProvider>();
            Assert.That(restored.serializableConverters, Has.Count.EqualTo(1));
            var converter = restored.serializableConverters[0] as
                EcsEntityViewConverter;
            Assert.IsNotNull(converter);
            Assert.AreEqual("EmbeddedView", converter.viewId);
            Assert.AreSame(
                prefab.GetComponent<EmptyView>(),
                converter.view);
            Assert.IsInstanceOf<EcsSerializableConverter<Main>>(
                new EcsEntityViewConverter());
        }

        [Test]
        public void SourceRelation_MaintainsReverseLinks()
        {
            CreateWorld();
            var source = World<ViewTestWorld>.NewEntity<Default>();
            var view = World<ViewTestWorld>.NewEntity<Default>();

            view.Set(new World<ViewTestWorld>.Link<ViewSource>(source.GID));

            Assert.IsTrue(source.Has<World<ViewTestWorld>.Links<SourceViews>>());
            view.Delete<World<ViewTestWorld>.Link<ViewSource>>();
            Assert.IsFalse(source.Has<World<ViewTestWorld>.Links<SourceViews>>());
        }

        [Test]
        public void ParentRelation_MaintainsReverseLinks()
        {
            CreateWorld();
            var parent = World<ViewTestWorld>.NewEntity<Default>();
            var child = World<ViewTestWorld>.NewEntity<Default>();

            child.Set(new World<ViewTestWorld>.Link<ViewParent>(parent.GID));

            Assert.IsTrue(parent.Has<World<ViewTestWorld>.Links<ViewChildren>>());
            child.Delete<World<ViewTestWorld>.Link<ViewParent>>();
            Assert.IsFalse(parent.Has<World<ViewTestWorld>.Links<ViewChildren>>());
        }

        [Test]
        public void Container_EnforcesCapacityAndBusyReuse()
        {
            CreateWorld();
            var host = new GameObject("ViewContainer");
            try
            {
                var registry = new ViewContainerRegistryResource<ViewTestWorld>();
                registry.Register("Hud", host.transform, 1);
                Assert.IsTrue(registry.TryReserve(
                    "Hud",
                    new ViewKey(1),
                    false,
                    out var parent));
                Assert.AreSame(host.transform, parent);
                Assert.IsFalse(registry.TryReserve(
                    "Hud",
                    new ViewKey(2),
                    false,
                    out _));
                Assert.IsTrue(registry.TryReserve(
                    "Hud",
                    new ViewKey(2),
                    true,
                    out _));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(host);
            }
        }

        private static void CreateWorld()
        {
            World<ViewTestWorld>.Create();
            var types = World<ViewTestWorld>.Types();
            types.RegisterAll(typeof(ViewComponent).Assembly);
            types.Component<OpenTestScreenComponent>();
            World<ViewTestWorld>.SetResource(
                new ViewKeySequenceResource<ViewTestWorld>());
            World<ViewTestWorld>.Initialize();
        }

        private static ViewRequest Request()
        {
            return new ViewRequest
            {
                ViewId = "TestView",
                Placement = ViewPlacement.InLayout(ViewType.Overlay),
                ShowOnOpen = true
            };
        }

        private struct ViewTestWorld : IWorldType
        {
        }

        [Test]
        public void CloseAllByType_EmitsExactViewTypeFilter()
        {
            CreateWorld();
            var receiver = World<ViewTestWorld>.RegisterEventReceiver<CloseViewsEvent>();
            try
            {
                Assert.IsTrue(ViewOperations.CloseAllByType<ViewTestWorld, EmptyView>());
                foreach (var received in receiver)
                {
                    var filter = received.Value.Filter;
                    Assert.AreEqual(typeof(EmptyView).AssemblyQualifiedName, filter.ViewType);
                    Assert.IsFalse(filter.All);
                    Assert.IsTrue(string.IsNullOrEmpty(filter.ViewId));
                    return;
                }

                Assert.Fail("No close-by-type request was sent.");
            }
            finally
            {
                World<ViewTestWorld>.DeleteEventReceiver(ref receiver);
            }
        }

        private struct OpenTestScreenComponent : IComponent
        {
        }

        private enum TestViewId
        {
            [ViewSystemId("inventory.screen")]
            InventoryAlias = 1,
            Unmapped = 2
        }

        private sealed class TestViewModel : ViewModel
        {
        }

        private sealed class OtherTestViewModel : ViewModel
        {
        }

        private sealed class OtherTestView : EmptyView
        {
        }
    }
}
