namespace UniGame.StaticEcs.Features
{
    using System.Collections.Generic;
    using FFS.Libraries.StaticEcs;
    using UnityEngine;

    /// <summary>Publishes every queued mouse button transition for live element entities.</summary>
    public class PublishUiToolkitPointerEventsSystem<TWorld> : ISystem
        where TWorld : struct, IWorldType
    {
        private readonly Queue<PointerTransition> _pending = new();
        private readonly Dictionary<EcsUiToolkitElement, EntityGID> _entities = new();

        public void Init()
        {
            EcsUiToolkitElement.PointerPressed += OnPressed;
            EcsUiToolkitElement.PointerReleased += OnReleased;
        }

        public void Update()
        {
            if (_pending.Count == 0)
                return;

            _entities.Clear();
            var filter = default(All<UiToolkitElementComponent>);
            foreach (var entity in World<TWorld>.Query(filter).Entities())
            {
                var element = entity.Read<UiToolkitElementComponent>().Element;
                if (element != null)
                    _entities[element] = entity.GID;
            }

            while (_pending.Count > 0)
            {
                var input = _pending.Dequeue();
                if (!_entities.TryGetValue(input.Element, out var entityId))
                    continue;

                World<TWorld>.SendEvent(new UiToolkitPointerButtonEvent
                {
                    Element = entityId,
                    IsPressed = input.IsPressed,
                    PointerId = input.PointerId,
                    Button = input.Button,
                    Position = input.Position
                });
            }

            _entities.Clear();
        }

        public void Destroy()
        {
            EcsUiToolkitElement.PointerPressed -= OnPressed;
            EcsUiToolkitElement.PointerReleased -= OnReleased;
            _pending.Clear();
            _entities.Clear();
        }

        private void OnPressed(EcsUiToolkitElement element, int pointerId, int button,
            Vector2 position) => Enqueue(element, true, pointerId, button, position);

        private void OnReleased(EcsUiToolkitElement element, int pointerId, int button,
            Vector2 position) => Enqueue(element, false, pointerId, button, position);

        private void Enqueue(EcsUiToolkitElement element, bool isPressed, int pointerId,
            int button, Vector2 position)
        {
            _pending.Enqueue(new PointerTransition
            {
                Element = element,
                IsPressed = isPressed,
                PointerId = pointerId,
                Button = button,
                Position = position
            });
        }

        private struct PointerTransition
        {
            public EcsUiToolkitElement Element;
            public bool IsPressed;
            public int PointerId;
            public int Button;
            public Vector2 Position;
        }
    }
}
