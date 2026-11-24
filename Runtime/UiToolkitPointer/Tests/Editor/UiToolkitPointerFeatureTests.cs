namespace UniGame.StaticEcs.Features.Tests
{
    using System.Collections.Generic;
    using FFS.Libraries.StaticEcs;
    using NUnit.Framework;
    using UnityEngine;
    using UnityEngine.UIElements;

    [TestFixture]
    public sealed class UiToolkitPointerFeatureTests
    {
        private struct PointerTestWorld : IWorldType
        {
        }

        private PublishUiToolkitPointerEventsSystem<PointerTestWorld> _system;
        private EventReceiver<PointerTestWorld, UiToolkitPointerButtonEvent> _receiver;

        [SetUp]
        public void SetUp()
        {
            World<PointerTestWorld>.Create(WorldConfig.Default());
            World<PointerTestWorld>.Types().Component<UiToolkitElementComponent>();
            World<PointerTestWorld>.Types().Event<UiToolkitPointerButtonEvent>();
            World<PointerTestWorld>.Initialize();
            _receiver = World<PointerTestWorld>
                .RegisterEventReceiver<UiToolkitPointerButtonEvent>();
            _system = new PublishUiToolkitPointerEventsSystem<PointerTestWorld>();
            _system.Init();
        }

        [TearDown]
        public void TearDown()
        {
            _system?.Destroy();
            _system = null;
            if (World<PointerTestWorld>.Status == WorldStatus.NotCreated)
                return;

            World<PointerTestWorld>.DeleteEventReceiver(ref _receiver);
            World<PointerTestWorld>.Destroy();
        }

        [Test]
        public void PublishesEveryMouseButtonTransitionInOrderWithinOneUpdate()
        {
            var firstElement = new TestElement();
            var secondElement = new TestElement();
            var first = World<PointerTestWorld>.NewEntity<Default>();
            first.Set(new UiToolkitElementComponent { Element = firstElement });
            var second = World<PointerTestWorld>.NewEntity<Default>();
            second.Set(new UiToolkitElementComponent { Element = secondElement });

            firstElement.Press(PointerId.mousePointerId, 0, new Vector2(10, 20));
            secondElement.Press(PointerId.mousePointerId, 1, new Vector2(30, 40));
            firstElement.Release(PointerId.mousePointerId, 0, new Vector2(11, 21));
            secondElement.Release(PointerId.mousePointerId, 1, new Vector2(31, 41));
            _system.Update();

            var received = new List<UiToolkitPointerButtonEvent>();
            foreach (var item in _receiver)
                received.Add(item.Value);

            Assert.That(received.Count, Is.EqualTo(4));
            Assert.That(received[0].Element, Is.EqualTo(first.GID));
            Assert.That(received[0].IsPressed, Is.True);
            Assert.That(received[0].Button, Is.EqualTo(0));
            Assert.That(received[0].PointerId, Is.EqualTo(PointerId.mousePointerId));
            Assert.That(received[0].Position, Is.EqualTo(new Vector2(10, 20)));
            Assert.That(received[1].Element, Is.EqualTo(second.GID));
            Assert.That(received[1].IsPressed, Is.True);
            Assert.That(received[1].Button, Is.EqualTo(1));
            Assert.That(received[2].Element, Is.EqualTo(first.GID));
            Assert.That(received[2].IsPressed, Is.False);
            Assert.That(received[2].Position, Is.EqualTo(new Vector2(11, 21)));
            Assert.That(received[3].Element, Is.EqualTo(second.GID));
            Assert.That(received[3].IsPressed, Is.False);

            _system.Update();
            foreach (var _ in _receiver)
                Assert.Fail("A pointer transition was published twice.");
        }

        [Test]
        public void IgnoresOtherPointersAndElementsWithoutLiveEntities()
        {
            var element = new TestElement();
            var entity = World<PointerTestWorld>.NewEntity<Default>();
            entity.Set(new UiToolkitElementComponent { Element = element });
            var unregistered = new TestElement();

            element.Press(PointerId.mousePointerId + 1, 0, Vector2.zero);
            unregistered.Press(PointerId.mousePointerId, 0, Vector2.zero);
            element.Press(PointerId.mousePointerId, 0, Vector2.zero);
            entity.Destroy();
            _system.Update();

            foreach (var _ in _receiver)
                Assert.Fail("An invalid element published a pointer event.");

            element.StopTracking();
            element.Release(PointerId.mousePointerId, 0, Vector2.zero);
            _system.Update();
            foreach (var _ in _receiver)
                Assert.Fail("A disposed element published a pointer event.");
        }

        private sealed class TestElement : EcsUiToolkitElement<TestElement>
        {
            public void Press(int pointerId, int button, Vector2 position) =>
                ReportPointerPressed(pointerId, button, position);

            public void Release(int pointerId, int button, Vector2 position) =>
                ReportPointerReleased(pointerId, button, position);

            public void StopTracking() => StopPointerTracking();
        }
    }
}
