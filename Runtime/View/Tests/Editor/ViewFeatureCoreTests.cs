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
        public void Placement_AcceptsBuiltInAndCustomLayouts()
        {
            Assert.AreEqual(
                "Overlay",
                ViewPlacement.InLayout(ViewType.Overlay).Layout);
            Assert.AreEqual(
                "CombatOverlay",
                ViewPlacement.InLayout("CombatOverlay").Layout);
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

        private sealed class TestViewModel : ViewModel
        {
        }
    }
}
