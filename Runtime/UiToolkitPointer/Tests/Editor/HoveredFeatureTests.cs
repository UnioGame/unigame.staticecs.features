namespace UniGame.StaticEcs.Features.Tests
{
    using FFS.Libraries.StaticEcs;
    using NUnit.Framework;

    [TestFixture]
    public sealed class HoveredFeatureTests
    {
        private struct HoveredTestWorld : IWorldType
        {
        }

        private sealed class TestElement : EcsUiToolkitElement<TestElement>
        {
        }

        [TearDown]
        public void TearDown()
        {
            if (World<HoveredTestWorld>.Status != WorldStatus.NotCreated)
                World<HoveredTestWorld>.Destroy();
        }

        [Test]
        public void TagFollowsEachElementPointerState()
        {
            World<HoveredTestWorld>.Create(WorldConfig.Default());
            World<HoveredTestWorld>.Types().Component<UiToolkitElementComponent>();
            World<HoveredTestWorld>.Types().Tag<IsHoveredTag>();
            World<HoveredTestWorld>.Initialize();

            var firstElement = new TestElement();
            var secondElement = new TestElement();
            var first = World<HoveredTestWorld>.NewEntity<Default>();
            first.Set(new UiToolkitElementComponent { Element = firstElement });
            var second = World<HoveredTestWorld>.NewEntity<Default>();
            second.Set(new UiToolkitElementComponent { Element = secondElement });
            var system = new UpdateHoveredElementsSystem<HoveredTestWorld>();

            firstElement.SetPointerInside(true);
            system.Update();
            Assert.That(first.Has<IsHoveredTag>(), Is.True);
            Assert.That(second.Has<IsHoveredTag>(), Is.False);

            firstElement.SetPointerInside(false);
            secondElement.SetPointerInside(true);
            system.Update();
            Assert.That(first.Has<IsHoveredTag>(), Is.False);
            Assert.That(second.Has<IsHoveredTag>(), Is.True);

            second.Destroy();
            system.Update();
            Assert.That(first.Has<IsHoveredTag>(), Is.False);
        }
    }
}
