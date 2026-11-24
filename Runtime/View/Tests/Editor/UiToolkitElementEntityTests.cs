namespace UniGame.StaticEcs.Features.Tests
{
    using System;
    using System.Text.RegularExpressions;
    using FFS.Libraries.StaticEcs;
    using NUnit.Framework;
    using UniGame.UiSystem.Runtime.UiToolkit;
    using UnityEngine;
    using UnityEngine.TestTools;
    using UnityEngine.UIElements;

    [TestFixture]
    public sealed class UiToolkitElementEntityTests
    {
        private SyncUiToolkitElementsSystem<ElementTestWorld> _activeSystem;

        [TearDown]
        public void TearDown()
        {
            _activeSystem?.Destroy();
            _activeSystem = null;
            if (World<ElementTestWorld>.Status != WorldStatus.NotCreated)
                World<ElementTestWorld>.Destroy();
        }

        [Test]
        public void RegistersDynamicElementAndReleasesItWithItsLifetime()
        {
            World<ElementTestWorld>.Create();
            World<ElementTestWorld>.Types().RegisterAll(typeof(ViewComponent).Assembly);
            World<ElementTestWorld>.Types()
                .Component<UiToolkitElementComponent<TestElement>>();
            var registry = new UiToolkitElementRegistryResource<ElementTestWorld>();
            World<ElementTestWorld>.SetResource(registry);
            World<ElementTestWorld>.Initialize();

            var viewEntity = World<ElementTestWorld>.NewEntity<Default>();
            var root = new VisualElement();
            var existing = new TestElement();
            root.Add(existing);
            var key = new ViewKey(3);
            registry.Register(key, viewEntity.GID, new FakeToolkitView { Root = root });
            _activeSystem = new SyncUiToolkitElementsSystem<ElementTestWorld>();
            _activeSystem.Init();
            _activeSystem.Update();
            var existingId = existing.Entity;

            var added = new TestElement();
            var converter = new TestConverter();
            added.Converters.Add(converter);
            root.Add(added);
            added.RequestEntityRegistration();
            _activeSystem.Update();

            Assert.AreEqual(existingId, existing.Entity);
            Assert.IsTrue(added.Entity.TryUnpack<ElementTestWorld>(out _));
            Assert.AreEqual(1, converter.Applied);
            Assert.AreEqual(2, registry.Entries[key].Children.Count);
            Assert.AreEqual(2,
                viewEntity.Read<World<ElementTestWorld>.Links<UiToolkitChildElements>>().Length);

            var addedId = added.Entity;
            added.RequestEntityRegistration();
            _activeSystem.Update();
            Assert.AreEqual(addedId, added.Entity);
            Assert.AreEqual(1, converter.Applied);

            var nestedContainer = new VisualElement();
            root.Add(nestedContainer);
            nestedContainer.Add(added);
            added.RequestEntityRegistration();
            _activeSystem.Update();
            Assert.AreEqual(addedId, added.Entity);

            added.StopTracking();
            nestedContainer.Remove(added);
            _activeSystem.Update();

            Assert.AreEqual(default(EntityGID), added.Entity);
            Assert.IsFalse(addedId.TryUnpack<ElementTestWorld>(out _));
            Assert.AreEqual(1, converter.Released);
            Assert.AreEqual(existingId, existing.Entity);
            Assert.AreEqual(1, registry.Entries[key].Children.Count);
        }

        [Test]
        public void ConverterFailureRollsBackEntityAndRetriesWithoutSkippingSiblings()
        {
            World<ElementTestWorld>.Create();
            World<ElementTestWorld>.Types().RegisterAll(typeof(ViewComponent).Assembly);
            World<ElementTestWorld>.Types()
                .Component<UiToolkitElementComponent<TestElement>>();
            var registry = new UiToolkitElementRegistryResource<ElementTestWorld>();
            World<ElementTestWorld>.SetResource(registry);
            World<ElementTestWorld>.Initialize();

            var viewEntity = World<ElementTestWorld>.NewEntity<Default>();
            var failed = new TestElement();
            var cleanup = new TestConverter();
            var failOnce = new FailOnceConverter();
            failed.Converters.Add(cleanup);
            failed.Converters.Add(failOnce);
            var healthy = new TestElement();
            var root = new VisualElement();
            root.Add(failed);
            root.Add(healthy);
            var key = new ViewKey(9);
            registry.Register(key, viewEntity.GID, new FakeToolkitView { Root = root });
            _activeSystem = new SyncUiToolkitElementsSystem<ElementTestWorld>();
            _activeSystem.Init();

            // Unity logs a wrapped exception under its innermost message; the wrapper goes to the trace.
            LogAssert.Expect(LogType.Exception, new Regex("Transient converter failure"));
            _activeSystem.Update();

            Assert.That(failed.Entity, Is.EqualTo(default(EntityGID)));
            Assert.That(healthy.Entity.TryUnpack<ElementTestWorld>(out _), Is.True);
            Assert.That(cleanup.Released, Is.EqualTo(1));
            Assert.That(registry.Entries[key].Children.Count, Is.EqualTo(1));
            var count = 0;
            foreach (var _ in World<ElementTestWorld>
                         .Query<All<UiToolkitElementComponent<TestElement>>>().Entities())
                count++;
            Assert.That(count, Is.EqualTo(1));

            _activeSystem.Update();
            Assert.That(failed.Entity.TryUnpack<ElementTestWorld>(out _), Is.True);
            Assert.That(registry.Entries[key].Children.Count, Is.EqualTo(2));
            Assert.That(failOnce.Applied, Is.EqualTo(2));
        }

        [Test]
        public void CleanupFailureStillReleasesElementAndRunsRemainingHandlers()
        {
            World<ElementTestWorld>.Create();
            World<ElementTestWorld>.Types().RegisterAll(typeof(ViewComponent).Assembly);
            World<ElementTestWorld>.Types()
                .Component<UiToolkitElementComponent<TestElement>>();
            var registry = new UiToolkitElementRegistryResource<ElementTestWorld>();
            World<ElementTestWorld>.SetResource(registry);
            World<ElementTestWorld>.Initialize();

            var viewEntity = World<ElementTestWorld>.NewEntity<Default>();
            var root = new VisualElement();
            var element = new TestElement();
            var remaining = new TestConverter();
            element.Converters.Add(new ThrowingCleanupConverter());
            element.Converters.Add(remaining);
            root.Add(element);
            var view = new FakeToolkitView { Root = root };
            var key = new ViewKey(10);
            registry.Register(key, viewEntity.GID, view);
            _activeSystem = new SyncUiToolkitElementsSystem<ElementTestWorld>();
            _activeSystem.Init();
            _activeSystem.Update();
            var elementId = element.Entity;

            view.Root = new VisualElement();
            LogAssert.Expect(LogType.Exception, new Regex("Expected cleanup failure"));
            _activeSystem.Update();

            Assert.That(element.Entity, Is.EqualTo(default(EntityGID)));
            Assert.That(elementId.TryUnpack<ElementTestWorld>(out _), Is.False);
            Assert.That(registry.Entries[key].Children, Is.Empty);
            Assert.That(remaining.Released, Is.EqualTo(1));
        }

        [Test]
        public void RegistersDynamicParentsBeforeTheirChildren()
        {
            World<ElementTestWorld>.Create();
            World<ElementTestWorld>.Types().RegisterAll(typeof(ViewComponent).Assembly);
            World<ElementTestWorld>.Types()
                .Component<UiToolkitElementComponent<TestElement>>();
            var registry = new UiToolkitElementRegistryResource<ElementTestWorld>();
            World<ElementTestWorld>.SetResource(registry);
            World<ElementTestWorld>.Initialize();

            var viewEntity = World<ElementTestWorld>.NewEntity<Default>();
            var root = new VisualElement();
            registry.Register(new ViewKey(6), viewEntity.GID,
                new FakeToolkitView { Root = root });
            _activeSystem = new SyncUiToolkitElementsSystem<ElementTestWorld>();
            _activeSystem.Init();
            _activeSystem.Update();

            var parent = new TestElement();
            var child = new TestElement();
            var converter = new ParentEntityConverter { Parent = parent };
            child.Converters.Add(converter);
            parent.Add(new VisualElement { name = "wrapper" });
            parent.Q<VisualElement>("wrapper").Add(child);
            root.Add(parent);
            child.RequestEntityRegistration();
            parent.RequestEntityRegistration();
            _activeSystem.Update();

            Assert.That(parent.Entity.TryUnpack<ElementTestWorld>(out _), Is.True);
            Assert.That(child.Entity.TryUnpack<ElementTestWorld>(out _), Is.True);
            Assert.That(converter.ParentWasRegistered, Is.True);
        }

        [Test]
        public void ReparentingDynamicElementTransfersItToTheNewView()
        {
            World<ElementTestWorld>.Create();
            World<ElementTestWorld>.Types().RegisterAll(typeof(ViewComponent).Assembly);
            World<ElementTestWorld>.Types()
                .Component<UiToolkitElementComponent<TestElement>>();
            var registry = new UiToolkitElementRegistryResource<ElementTestWorld>();
            World<ElementTestWorld>.SetResource(registry);
            World<ElementTestWorld>.Initialize();

            var firstView = World<ElementTestWorld>.NewEntity<Default>();
            var secondView = World<ElementTestWorld>.NewEntity<Default>();
            var firstRoot = new VisualElement();
            var secondRoot = new VisualElement();
            var element = new TestElement();
            firstRoot.Add(element);
            registry.Register(new ViewKey(4), firstView.GID,
                new FakeToolkitView { Root = firstRoot });
            registry.Register(new ViewKey(5), secondView.GID,
                new FakeToolkitView { Root = secondRoot });
            _activeSystem = new SyncUiToolkitElementsSystem<ElementTestWorld>();
            _activeSystem.Init();
            _activeSystem.Update();
            var oldId = element.Entity;

            secondRoot.Add(element);
            element.RequestEntityRegistration();
            _activeSystem.Update();

            Assert.IsFalse(oldId.TryUnpack<ElementTestWorld>(out _));
            Assert.IsTrue(element.Entity.TryUnpack<ElementTestWorld>(out var newEntity));
            Assert.AreEqual(secondView.GID,
                newEntity.Read<World<ElementTestWorld>.Link<UiToolkitElementParent>>().Value);
            Assert.IsFalse(firstView.Has<World<ElementTestWorld>.Links<UiToolkitChildElements>>());
            Assert.AreEqual(1,
                secondView.Read<World<ElementTestWorld>.Links<UiToolkitChildElements>>().Length);
        }

        [Test]
        public void CreatesDistinctEntitiesAndReleasesThemOnRootChangeAndClose()
        {
            World<ElementTestWorld>.Create();
            World<ElementTestWorld>.Types().RegisterAll(typeof(ViewComponent).Assembly);
            World<ElementTestWorld>.Types()
                .Component<UiToolkitElementComponent<TestElement>>();
            var registry = new UiToolkitElementRegistryResource<ElementTestWorld>();
            World<ElementTestWorld>.SetResource(registry);
            World<ElementTestWorld>.Initialize();

            var viewEntity = World<ElementTestWorld>.NewEntity<Default>();
            var first = new TestElement();
            var converter = new TestConverter();
            first.Converters.Add(converter);
            var second = new TestElement();
            var root = new VisualElement();
            root.Add(first);
            root.Add(new VisualElement());
            root.Add(second);

            var view = new FakeToolkitView { Root = root };
            var key = new ViewKey(1);
            registry.Register(key, viewEntity.GID, view);
            var system = new SyncUiToolkitElementsSystem<ElementTestWorld>();

            system.Update();

            Assert.AreNotEqual(default(EntityGID), first.Entity);
            Assert.AreNotEqual(first.Entity, second.Entity);
            Assert.IsTrue(first.Entity.TryUnpack<ElementTestWorld>(out var firstEntity));
            Assert.AreSame(first, firstEntity.Read<UiToolkitElementComponent>().Element);
            Assert.IsTrue(firstEntity.Has<UiToolkitElementComponent<TestElement>>());
            Assert.AreEqual(1, converter.Applied);
            Assert.AreEqual(viewEntity.GID,
                firstEntity.Read<World<ElementTestWorld>.Link<UiToolkitElementParent>>().Value);
            Assert.AreEqual(2, registry.Entries[key].Children.Count);
            Assert.AreEqual(2,
                viewEntity.Read<World<ElementTestWorld>.Links<UiToolkitChildElements>>().Length);

            system.Update();
            Assert.AreEqual(2, registry.Entries[key].Children.Count);

            var oldFirst = first.Entity;
            var replacement = new TestElement();
            var newRoot = new VisualElement();
            newRoot.Add(replacement);
            view.Root = newRoot;
            system.Update();

            Assert.AreEqual(default(EntityGID), first.Entity);
            Assert.AreEqual(1, converter.Released);
            Assert.IsFalse(oldFirst.TryUnpack<ElementTestWorld>(out _));
            Assert.IsTrue(replacement.Entity.TryUnpack<ElementTestWorld>(out _));
            Assert.AreEqual(1, registry.Entries[key].Children.Count);
            Assert.AreEqual(1,
                viewEntity.Read<World<ElementTestWorld>.Links<UiToolkitChildElements>>().Length);

            var replacementGid = replacement.Entity;
            registry.Unregister(key);
            system.Update();

            Assert.AreEqual(default(EntityGID), replacement.Entity);
            Assert.IsFalse(replacementGid.TryUnpack<ElementTestWorld>(out _));
            Assert.IsFalse(registry.Entries.ContainsKey(key));
            Assert.IsFalse(viewEntity.Has<World<ElementTestWorld>.Links<UiToolkitChildElements>>());
        }

        [Test]
        public void OwnerDestructionReleasesElementEntity()
        {
            World<ElementTestWorld>.Create();
            World<ElementTestWorld>.Types().RegisterAll(typeof(ViewComponent).Assembly);
            World<ElementTestWorld>.Types()
                .Component<UiToolkitElementComponent<TestElement>>();
            var registry = new UiToolkitElementRegistryResource<ElementTestWorld>();
            World<ElementTestWorld>.SetResource(registry);
            World<ElementTestWorld>.Initialize();

            var viewEntity = World<ElementTestWorld>.NewEntity<Default>();
            var element = new TestElement();
            var root = new VisualElement();
            root.Add(element);
            var key = new ViewKey(2);
            registry.Register(key, viewEntity.GID, new FakeToolkitView { Root = root });
            var system = new SyncUiToolkitElementsSystem<ElementTestWorld>();
            system.Update();
            var elementGid = element.Entity;

            viewEntity.Destroy();
            system.Update();

            Assert.AreEqual(default(EntityGID), element.Entity);
            Assert.IsFalse(elementGid.TryUnpack<ElementTestWorld>(out _));
            Assert.IsFalse(registry.Entries.ContainsKey(key));
        }

        private struct ElementTestWorld : IWorldType
        {
        }

        private sealed class TestElement : EcsUiToolkitElement<TestElement>
        {
            public void StopTracking() => StopPointerTracking();
        }

        private sealed class TestConverter : UiToolkitElementConverter,
            IUiToolkitElementDestroyHandler
        {
            public int Applied;
            public int Released;

            public override void Apply<TWorld>(World<TWorld>.Entity entity)
            {
                Applied++;
            }

            public void OnEntityDestroy<TWorld>(World<TWorld>.Entity entity)
                where TWorld : struct, IWorldType
            {
                Released++;
            }
        }

        private sealed class FailOnceConverter : UiToolkitElementConverter
        {
            public int Applied;

            public override void Apply<TWorld>(World<TWorld>.Entity entity)
            {
                Applied++;
                if (Applied == 1)
                    throw new InvalidOperationException("Transient converter failure.");
            }
        }

        private sealed class ThrowingCleanupConverter : UiToolkitElementConverter,
            IUiToolkitElementDestroyHandler
        {
            public override void Apply<TWorld>(World<TWorld>.Entity entity)
            {
            }

            public void OnEntityDestroy<TWorld>(World<TWorld>.Entity entity)
                where TWorld : struct, IWorldType =>
                throw new InvalidOperationException("Expected cleanup failure.");
        }

        private sealed class ParentEntityConverter : UiToolkitElementConverter
        {
            public TestElement Parent;
            public bool ParentWasRegistered;

            public override void Apply<TWorld>(World<TWorld>.Entity entity)
            {
                ParentWasRegistered = Parent.Entity.TryUnpack<TWorld>(out _);
            }
        }

        private sealed class FakeToolkitView : IUiToolkitView
        {
            public bool AcceptInput { get; set; }
            public PanelRenderer PanelRenderer => null;
            public VisualElement Root { get; set; }

            public void AddUnbind(Action unbind) { }
            public void SetVisible(bool visible) { }
            public void SetStackInputAllowed(bool allowed) { }
            public void PrepareForPanelReload() { }
            public void FocusFirstFocusable() { }
        }
    }
}
