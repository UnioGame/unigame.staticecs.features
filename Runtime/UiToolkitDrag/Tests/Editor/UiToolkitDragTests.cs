namespace UniGame.StaticEcs.Features.Tests
{
    using FFS.Libraries.StaticEcs;
    using NUnit.Framework;
    using UnityEngine;
    using UnityEngine.UIElements;

    public struct UiToolkitDragTestWorld : IWorldType
    {
    }

    public partial class GenericDragTestElement : EcsUiToolkitElement<GenericDragTestElement>
    {
    }

    [TestFixture]
    public sealed class UiToolkitDragTests
    {
        [SetUp]
        public void SetUp()
        {
            World<UiToolkitDragTestWorld>.Create(WorldConfig.Default());
            var types = World<UiToolkitDragTestWorld>.Types();
            types.Component<DragObserveComponent>();
            types.Component<DragableComponent>();
            types.Component<DragSlotComponent>();
            types.Component<DragSlotMembershipComponent>();
            types.Component<DragTargetComponent>();
            types.Component<UiToolkitElementComponent>();
            types.Tag<IsDragStateTag>();
            types.Tag<DragBlockedTag>();
            types.Event<UiToolkitPointerButtonEvent>();
            types.Event<UiToolkitDragDropEvent>();
            World<UiToolkitDragTestWorld>.Initialize();
            World<UiToolkitDragTestWorld>.SetResource(new UiToolkitDragInputResource<UiToolkitDragTestWorld>
            {
                RuntimePolling = false,
                IsPressed = true
            });
            World<UiToolkitDragTestWorld>.SetResource(new UiToolkitDragVisualResource<UiToolkitDragTestWorld>());
        }

        [TearDown]
        public void TearDown()
        {
            if (World<UiToolkitDragTestWorld>.Status != WorldStatus.NotCreated)
                World<UiToolkitDragTestWorld>.Destroy();
        }

        [Test]
        public void ThresholdAndCancellationClearDragState()
        {
            var item = World<UiToolkitDragTestWorld>.NewEntity<Default>();
            item.Set(new DragableComponent { Mask = 1 });
            item.Set(new UiToolkitElementComponent());
            item.Set(new DragObserveComponent { StartScreen = Vector2.zero });
            var input = World<UiToolkitDragTestWorld>
                .GetResource<UiToolkitDragInputResource<UiToolkitDragTestWorld>>();
            var motion = new UpdateUiToolkitDragSystem<UiToolkitDragTestWorld>();
            var completion = new CompleteUiToolkitDragSystem<UiToolkitDragTestWorld>();
            completion.Init();

            input.Screen = new Vector2(7, 0);
            motion.Update();
            Assert.That(item.Has<IsDragStateTag>(), Is.False);
            input.Screen = new Vector2(8, 0);
            motion.Update();
            Assert.That(item.Has<IsDragStateTag>(), Is.True);
            input.Cancelled = true;
            completion.Update();
            Assert.That(item.Has<DragObserveComponent>(), Is.False);
            Assert.That(item.Has<IsDragStateTag>(), Is.False);
            completion.Destroy();
        }

        [Test]
        public void ReleaseFromAnotherPointerDoesNotCompleteActiveDrag()
        {
            var source = World<UiToolkitDragTestWorld>.NewEntity<Default>();
            var target = World<UiToolkitDragTestWorld>.NewEntity<Default>();
            var item = World<UiToolkitDragTestWorld>.NewEntity<Default>();
            item.Set(new DragableComponent { Mask = 1 });
            item.Set(new DragObserveComponent { PointerId = 42, SourceSlot = source.GID });
            item.Set<IsDragStateTag>();
            item.Set(new DragTargetComponent { Slot = target.GID, CanDrop = true });
            var completion = new CompleteUiToolkitDragSystem<UiToolkitDragTestWorld>();
            var drops = World<UiToolkitDragTestWorld>.RegisterEventReceiver<UiToolkitDragDropEvent>();
            completion.Init();
            try
            {
                World<UiToolkitDragTestWorld>.SendEvent(new UiToolkitPointerButtonEvent
                {
                    PointerId = 43,
                    Button = 0,
                    IsPressed = false
                });
                completion.Update();
                Assert.That(item.Has<DragObserveComponent>(), Is.True);

                World<UiToolkitDragTestWorld>.SendEvent(new UiToolkitPointerButtonEvent
                {
                    PointerId = 42,
                    Button = 0,
                    IsPressed = false
                });
                completion.Update();
                Assert.That(item.Has<DragObserveComponent>(), Is.False);
                var count = 0;
                foreach (var drop in drops)
                {
                    Assert.That(drop.Value.SourceElement, Is.EqualTo(item.GID));
                    count++;
                }
                Assert.That(count, Is.EqualTo(1));
            }
            finally
            {
                completion.Destroy();
                World<UiToolkitDragTestWorld>.DeleteEventReceiver(ref drops);
            }
        }

        [Test]
        public void ObserveUsesExplicitSlotMembershipWithoutUiParent()
        {
            var source = World<UiToolkitDragTestWorld>.NewEntity<Default>();
            source.Set(new DragSlotComponent { AllowedMask = 1 });
            var item = World<UiToolkitDragTestWorld>.NewEntity<Default>();
            item.Set(new DragableComponent { Mask = 1 });
            item.Set(new DragSlotMembershipComponent { Slot = source.GID });
            item.Set(new UiToolkitElementComponent { Element = new GenericDragTestElement() });
            var observe = new ObserveUiToolkitDragSystem<UiToolkitDragTestWorld>();
            observe.Init();
            try
            {
                World<UiToolkitDragTestWorld>.SendEvent(new UiToolkitPointerButtonEvent
                {
                    Element = item.GID,
                    IsPressed = true,
                    Button = 0
                });
                observe.Update();
                Assert.That(item.Has<DragObserveComponent>(), Is.True);
                Assert.That(item.Read<DragObserveComponent>().SourceSlot, Is.EqualTo(source.GID));
            }
            finally
            {
                observe.Destroy();
            }
        }

        [Test]
        public void DropRulesUseEcsSlotMembershipAndBothSwapMasks()
        {
            var item = World<UiToolkitDragTestWorld>.NewEntity<Default>();
            item.Set(new DragableComponent { Mask = 2 });
            var source = World<UiToolkitDragTestWorld>.NewEntity<Default>();
            source.Set(new DragSlotComponent { AllowedMask = 1 });
            var target = World<UiToolkitDragTestWorld>.NewEntity<Default>();
            target.Set(new DragSlotComponent { AllowedMask = 2 });
            var element = new DragSlotToolkitElement();
            target.Set(new UiToolkitElementComponent { Element = element });
            var resolver = new ResolveUiToolkitDragTargetSystem<UiToolkitDragTestWorld>();

            Assert.That(resolver.CanDrop(item, source, target), Is.True);
            element.SetDropHoverFeedback(true, true);
            Assert.That(element.ClassListContains("drag-slot--drop-allowed"), Is.True);

            var occupant = World<UiToolkitDragTestWorld>.NewEntity<Default>();
            occupant.Set(new DragableComponent { Mask = 1 });
            occupant.Set(new DragSlotMembershipComponent { Slot = target.GID });
            var wrapper = new VisualElement();
            element.Add(wrapper);
            wrapper.Add(new GenericDragTestElement());
            Assert.That(resolver.CanDrop(item, source, target), Is.True);
            occupant.Mut<DragableComponent>().Mask = 4;
            Assert.That(resolver.CanDrop(item, source, target), Is.False);
            occupant.Mut<DragableComponent>().Mask = 1;
            occupant.Delete<DragSlotMembershipComponent>();
            var fixedOccupant = World<UiToolkitDragTestWorld>.NewEntity<Default>();
            fixedOccupant.Set(new DragSlotMembershipComponent { Slot = target.GID });
            Assert.That(resolver.CanDrop(item, source, target), Is.False);
            fixedOccupant.Delete<DragSlotMembershipComponent>();

            target.Mut<DragSlotComponent>().AllowedMask = 1;
            Assert.That(resolver.CanDrop(item, source, target), Is.False);
            element.SetDropHoverFeedback(true, false);
            Assert.That(element.ClassListContains("drag-slot--drop-allowed"), Is.False);
            Assert.That(element.ClassListContains("drag-slot--drop-blocked"), Is.True);

            target.Mut<DragSlotComponent>().AllowedMask = 2;
            target.Set<DragBlockedTag>();
            Assert.That(resolver.CanDrop(item, source, target), Is.False);
            target.Delete<DragBlockedTag>();
            Assert.That(resolver.CanDrop(item, source, source), Is.False);

            element.SetDropHoverFeedback(false, false);
            Assert.That(element.ClassListContains("drag-slot--drop-blocked"), Is.False);

            element.SetDropHoverFeedback(true, true);
            new UpdateUiToolkitDragSlotFeedbackSystem<UiToolkitDragTestWorld>().Update();
            Assert.That(element.ClassListContains("drag-slot--drop-allowed"), Is.False);
        }

        [Test]
        public void FeedbackWorksForAnyRegisteredSlotElement()
        {
            var element = new GenericDragTestElement();
            var slot = World<UiToolkitDragTestWorld>.NewEntity<Default>();
            slot.Set(new DragSlotComponent { AllowedMask = 1 });
            slot.Set(new UiToolkitElementComponent { Element = element });
            var item = World<UiToolkitDragTestWorld>.NewEntity<Default>();
            item.Set(new DragableComponent { Mask = 1 });
            item.Set(new DragObserveComponent());
            item.Set<IsDragStateTag>();
            item.Set(new DragTargetComponent { Slot = slot.GID, CanDrop = true });

            var feedback = new UpdateUiToolkitDragSlotFeedbackSystem<UiToolkitDragTestWorld>();
            feedback.Update();
            Assert.That(element.ClassListContains("drag-slot--drop-allowed"), Is.True);
            item.Delete<DragTargetComponent>();
            feedback.Update();
            Assert.That(element.ClassListContains("drag-slot--drop-allowed"), Is.False);
        }
    }
}
