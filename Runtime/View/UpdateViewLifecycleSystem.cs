namespace UniGame.StaticEcs.Features
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Threading;
    using Cysharp.Threading.Tasks;
    using FFS.Libraries.StaticEcs;
    using UniGame.StaticEcs.Unity;
    using UniGame.ViewSystem.Runtime;
    using UnityEngine;

    /// <summary>Orchestrates View System requests and mirrors their lifecycle into ECS.</summary>
    public class UpdateViewLifecycleSystem<TWorld> : ISystem
        where TWorld : struct, IWorldType
    {
        private readonly IGameViewSystem _viewSystem;
        private readonly ViewModelBinderRegistry<TWorld> _binders;
        private readonly Dictionary<ViewKey, RuntimeView> _views = new();
        private readonly Dictionary<string, Queue<ViewKey>> _queues = new();
        private readonly Dictionary<string, ViewKey> _activeQueues = new();
        private readonly ConcurrentQueue<OpenCompletion> _completions = new();
        private readonly List<ViewKey> _keys = new(32);
        private readonly CancellationTokenSource _cancellation = new();

        private EventReceiver<TWorld, OpenViewOnEntityEvent> _openOnReceiver;
        private EventReceiver<TWorld, OpenViewEvent> _openReceiver;
        private EventReceiver<TWorld, BindExistingViewEvent> _bindReceiver;
        private EventReceiver<TWorld, SetViewVisibilityEvent> _visibilityReceiver;
        private EventReceiver<TWorld, CloseViewEvent> _closeReceiver;
        private EventReceiver<TWorld, CloseViewsEvent> _closeAllReceiver;
        private long _updateIndex;

        /// <summary>Creates the lifecycle system for a live View System service.</summary>
        internal UpdateViewLifecycleSystem(
            IGameViewSystem viewSystem,
            ViewModelBinderRegistry<TWorld> binders)
        {
            _viewSystem = viewSystem ?? throw new ArgumentNullException(nameof(viewSystem));
            _binders = binders ?? throw new ArgumentNullException(nameof(binders));
        }

        /// <inheritdoc />
        public void Init()
        {
            _openOnReceiver = World<TWorld>.RegisterEventReceiver<OpenViewOnEntityEvent>();
            _openReceiver = World<TWorld>.RegisterEventReceiver<OpenViewEvent>();
            _bindReceiver = World<TWorld>.RegisterEventReceiver<BindExistingViewEvent>();
            _visibilityReceiver =
                World<TWorld>.RegisterEventReceiver<SetViewVisibilityEvent>();
            _closeReceiver = World<TWorld>.RegisterEventReceiver<CloseViewEvent>();
            _closeAllReceiver = World<TWorld>.RegisterEventReceiver<CloseViewsEvent>();
        }

        /// <inheritdoc />
        public void Update()
        {
            _updateIndex++;
            ApplyCompletions();
            AcceptRequests();
            ProcessCommands();
            RefreshRuntimeState();
            CleanupTerminalViews();
        }

        /// <inheritdoc />
        public void Destroy()
        {
            _cancellation.Cancel();
            SnapshotKeys();
            foreach (var key in _keys)
            {
                if (_views.TryGetValue(key, out var runtime))
                    Release(runtime, false);
            }

            _views.Clear();
            _queues.Clear();
            _activeQueues.Clear();
            _cancellation.Dispose();

            World<TWorld>.DeleteEventReceiver(ref _openOnReceiver);
            World<TWorld>.DeleteEventReceiver(ref _openReceiver);
            World<TWorld>.DeleteEventReceiver(ref _bindReceiver);
            World<TWorld>.DeleteEventReceiver(ref _visibilityReceiver);
            World<TWorld>.DeleteEventReceiver(ref _closeReceiver);
            World<TWorld>.DeleteEventReceiver(ref _closeAllReceiver);
        }

        private void AcceptRequests()
        {
            foreach (var received in _openOnReceiver)
            {
                var request = received.Value;
                if (!request.Entity.TryUnpack<TWorld>(out var entity) ||
                    entity.Has<ViewComponent>())
                {
                    NotifyFailed(request.Key, request.Entity, "Target entity is unavailable or already owns a view.");
                    continue;
                }

                Accept(request.Key, entity, request.Request, null, true, false);
            }

            foreach (var received in _openReceiver)
            {
                var request = received.Value;
                var entity = World<TWorld>.NewEntity<Default>();
                Accept(request.Key, entity, request.Request, null, true, true);
            }

            foreach (var received in _bindReceiver)
            {
                var request = received.Value;
                if (!request.Entity.TryUnpack<TWorld>(out var entity) ||
                    entity.Has<ViewComponent>() ||
                    request.View == null)
                {
                    NotifyFailed(request.Key, request.Entity, "Existing view binding target is unavailable.");
                    continue;
                }

                Accept(request.Key, entity, request.Request, request.View, false, false);
            }
        }

        private void Accept(
            ViewKey key,
            World<TWorld>.Entity entity,
            in ViewRequest request,
            IView existingView,
            bool ownsView,
            bool ownsEntity)
        {
            var queued = !string.IsNullOrWhiteSpace(request.Queue);
            var status = queued ? ViewLifecycleStatus.Queued : ViewLifecycleStatus.Opening;
            entity.Set(CreateComponent(key, request, status));
            AttachRelations(entity, request);

            var runtime = new RuntimeView
            {
                key = key,
                entity = entity.GID,
                request = request,
                view = existingView,
                ownsView = ownsView,
                ownsEntity = ownsEntity,
                status = status
            };
            _views.Add(key, runtime);
            NotifyStatus(runtime, status);

            if (queued)
            {
                Enqueue(runtime);
                return;
            }

            StartOpen(runtime);
        }

        private void Enqueue(RuntimeView runtime)
        {
            var queueName = runtime.request.Queue;
            if (!_queues.TryGetValue(queueName, out var queue))
            {
                queue = new Queue<ViewKey>();
                _queues.Add(queueName, queue);
            }

            queue.Enqueue(runtime.key);
            TryStartNext(queueName);
        }

        private void TryStartNext(string queueName)
        {
            if (_activeQueues.ContainsKey(queueName) ||
                !_queues.TryGetValue(queueName, out var queue))
                return;

            while (queue.Count > 0)
            {
                var key = queue.Dequeue();
                if (!_views.TryGetValue(key, out var runtime))
                    continue;

                _activeQueues[queueName] = key;
                SetStatus(runtime, ViewLifecycleStatus.Opening);
                StartOpen(runtime);
                return;
            }
        }

        private void StartOpen(RuntimeView runtime)
        {
            OpenAsync(runtime).Forget();
        }

        private async UniTaskVoid OpenAsync(RuntimeView runtime)
        {
            try
            {
                var view = runtime.view;
                if (view == null)
                    view = await CreateViewAsync(runtime);
                else if (!view.IsModelAttached)
                {
                    var model = await _viewSystem.CreateViewModel(runtime.request.ViewId);
                    view = await _viewSystem.InitializeView(view, model, null);
                }

                if (_cancellation.IsCancellationRequested)
                {
                    if (runtime.ownsView && view != null)
                        view.Close();

                    return;
                }

                _completions.Enqueue(new OpenCompletion
                {
                    key = runtime.key,
                    view = view,
                    ownsView = runtime.ownsView
                });
            }
            catch (OperationCanceledException)
            {
                _completions.Enqueue(new OpenCompletion
                {
                    key = runtime.key,
                    error = "View opening was cancelled."
                });
            }
            catch (Exception exception)
            {
                _completions.Enqueue(new OpenCompletion
                {
                    key = runtime.key,
                    error = exception.Message
                });
            }
        }

        private async UniTask<IView> CreateViewAsync(RuntimeView runtime)
        {
            var request = runtime.request;
            var placement = request.Placement;
            if (placement.Mode == ViewPlacementMode.Layout)
                return request.ShowOnOpen
                    ? await _viewSystem.Open(
                        request.ViewId,
                        placement.Layout,
                        request.Skin,
                        request.Name)
                    : await _viewSystem.Create(
                        request.ViewId,
                        placement.Layout,
                        request.Skin,
                        request.Name);

            var parent = ResolveParent(placement);
            if (placement.Mode == ViewPlacementMode.Container)
            {
                if (!World<TWorld>.HasResource<ViewContainerRegistryResource<TWorld>>() ||
                    !World<TWorld>.GetResource<ViewContainerRegistryResource<TWorld>>()
                        .TryReserve(
                            placement.Container,
                            runtime.key,
                            placement.UseBusyContainer,
                            out parent))
                    throw new InvalidOperationException(
                        $"Named View System container '{placement.Container}' is unavailable.");

                runtime.containerReserved = true;
            }

            var model = await _viewSystem.CreateViewModel(request.ViewId);
            return await _viewSystem.Create(
                model,
                request.ViewId,
                request.Skin,
                parent,
                request.Name,
                placement.StayWorldPosition);
        }

        private Transform ResolveParent(in ViewPlacement placement)
        {
            switch (placement.Mode)
            {
                case ViewPlacementMode.ParentView:
                    if (_views.TryGetValue(placement.ParentView, out var parentView) &&
                        parentView.view != null)
                        return parentView.view.Transform;

                    throw new InvalidOperationException("Parent view is unavailable.");
                case ViewPlacementMode.ParentEntity:
                    if (placement.ParentEntity.TryUnpack<TWorld>(out var parentEntity) &&
                        parentEntity.Has<TransformComponent>())
                        return parentEntity.Read<TransformComponent>().Transform;

                    throw new InvalidOperationException("Parent entity transform is unavailable.");
                case ViewPlacementMode.Transform:
                    return placement.Parent != null
                        ? placement.Parent
                        : throw new InvalidOperationException("Parent transform is unavailable.");
                default:
                    return null;
            }
        }

        private void ApplyCompletions()
        {
            while (_completions.TryDequeue(out var completion))
            {
                if (!_views.TryGetValue(completion.key, out var runtime))
                {
                    if (completion.view != null && completion.ownsView)
                        completion.view.Close();

                    continue;
                }

                if (runtime.terminalUpdate >= 0)
                {
                    if (completion.view != null && completion.ownsView)
                        completion.view.Close();

                    continue;
                }

                if (!string.IsNullOrEmpty(completion.error) ||
                    completion.view == null ||
                    !runtime.entity.TryUnpack<TWorld>(out var entity) ||
                    OwnerIsDead(runtime.request.Owner))
                {
                    if (completion.view != null && runtime.ownsView)
                        completion.view.Close();

                    Fail(runtime, completion.error ?? "View or its ECS owner became unavailable.");
                    continue;
                }

                var model = completion.view.ViewModel;
                if (model == null ||
                    !_binders.TryGet(model.GetType(), out var binder) ||
                    !binder.Attach(entity, model))
                {
                    if (runtime.ownsView)
                        completion.view.Close();

                    Fail(runtime, $"View model '{model?.GetType().FullName ?? "<null>"}' is not registered.");
                    continue;
                }

                runtime.view = completion.view;
                runtime.binder = binder;
                ApplyPlacement(runtime);
                var status = MapStatus(completion.view.Status.CurrentValue);
                SetStatus(runtime, status);
                World<TWorld>.SendEvent(new ViewOpenedEvent
                {
                    Key = runtime.key,
                    Entity = runtime.entity
                });
            }
        }

        private void ApplyPlacement(RuntimeView runtime)
        {
            var transform = runtime.view.Transform;
            var placement = runtime.request.Placement;
            if (transform == null)
                return;

            if (placement.ApplyPosition)
                if (placement.Space == ViewPlacementSpace.World)
                    transform.position = placement.Position;
                else
                    transform.localPosition = placement.Position;

            if (placement.ApplyRotation)
                if (placement.Space == ViewPlacementSpace.World)
                    transform.rotation = placement.Rotation;
                else
                    transform.localRotation = placement.Rotation;

            if (placement.ApplyScale)
                transform.localScale = placement.Scale;

            transform.SetSiblingIndex(Mathf.Max(0, runtime.request.Order));
        }

        private void ProcessCommands()
        {
            foreach (var received in _visibilityReceiver)
            {
                var request = received.Value;
                if (!_views.TryGetValue(request.Key, out var runtime) ||
                    runtime.view == null)
                    continue;

                if (request.Visible)
                {
                    SetStatus(runtime, ViewLifecycleStatus.Showing);
                    runtime.view.Show();
                }
                else
                {
                    SetStatus(runtime, ViewLifecycleStatus.Hiding);
                    runtime.view.Hide();
                }
            }

            foreach (var received in _closeReceiver)
            {
                Close(received.Value.Key);
            }

            foreach (var received in _closeAllReceiver)
            {
                var filter = received.Value.Filter;
                SnapshotKeys();
                foreach (var key in _keys)
                {
                    if (_views.TryGetValue(key, out var runtime) &&
                        Matches(runtime, filter))
                        Close(key);
                }
            }
        }

        private void RefreshRuntimeState()
        {
            SnapshotKeys();
            foreach (var key in _keys)
            {
                if (!_views.TryGetValue(key, out var runtime) ||
                    runtime.terminalUpdate >= 0)
                    continue;

                if (OwnerIsDead(runtime.request.Owner))
                {
                    Close(key);
                    continue;
                }

                if (runtime.request.Placement.Mode == ViewPlacementMode.ParentView &&
                    (!_views.TryGetValue(runtime.request.Placement.ParentView, out var parent) ||
                     parent.terminalUpdate >= 0))
                {
                    Close(key);
                    continue;
                }

                if (runtime.view == null)
                    continue;

                if (!runtime.view.IsAlive || runtime.view.IsTerminated)
                {
                    MarkClosed(runtime);
                    continue;
                }

                var status = MapStatus(runtime.view.Status.CurrentValue);
                if (status != runtime.status)
                    SetStatus(runtime, status);
            }
        }

        private void Close(ViewKey key)
        {
            if (!_views.TryGetValue(key, out var runtime) ||
                runtime.terminalUpdate >= 0)
                return;

            SetStatus(runtime, ViewLifecycleStatus.Closing);
            if (runtime.view != null && runtime.ownsView)
                runtime.view.Close();

            MarkClosed(runtime);
        }

        private void MarkClosed(RuntimeView runtime)
        {
            SetStatus(runtime, ViewLifecycleStatus.Closed);
            runtime.terminalUpdate = _updateIndex;
            World<TWorld>.SendEvent(new ViewClosedEvent
            {
                Key = runtime.key,
                Entity = runtime.entity
            });
            ReleaseQueue(runtime);
        }

        private void Fail(RuntimeView runtime, string reason)
        {
            SetStatus(runtime, ViewLifecycleStatus.Failed);
            runtime.terminalUpdate = _updateIndex;
            World<TWorld>.SendEvent(new ViewOpenFailedEvent
            {
                Key = runtime.key,
                Entity = runtime.entity,
                Reason = string.IsNullOrWhiteSpace(reason) ? "Unknown View System error." : reason
            });
            ReleaseQueue(runtime);
        }

        private void CleanupTerminalViews()
        {
            SnapshotKeys();
            foreach (var key in _keys)
            {
                if (!_views.TryGetValue(key, out var runtime) ||
                    runtime.terminalUpdate < 0 ||
                    runtime.terminalUpdate >= _updateIndex)
                    continue;

                Release(runtime, true);
                _views.Remove(key);
            }
        }

        private void Release(RuntimeView runtime, bool destroyOwnedEntity)
        {
            if (runtime.containerReserved &&
                World<TWorld>.HasResource<ViewContainerRegistryResource<TWorld>>())
            {
                World<TWorld>.GetResource<ViewContainerRegistryResource<TWorld>>()
                    .Release(runtime.request.Placement.Container, runtime.key);
                runtime.containerReserved = false;
            }

            if (runtime.entity.TryUnpack<TWorld>(out var entity))
            {
                runtime.binder?.Detach(entity);
                if (destroyOwnedEntity && runtime.ownsEntity)
                    entity.Destroy();
                else if (entity.Has<ViewComponent>())
                    entity.Delete<ViewComponent>();
            }

            if (runtime.ownsView && runtime.view != null &&
                runtime.view.IsAlive && !runtime.view.IsTerminated)
                runtime.view.Close();
        }

        private void ReleaseQueue(RuntimeView runtime)
        {
            var queue = runtime.request.Queue;
            if (string.IsNullOrWhiteSpace(queue))
                return;

            if (_activeQueues.TryGetValue(queue, out var active) &&
                active == runtime.key)
            {
                _activeQueues.Remove(queue);
                TryStartNext(queue);
            }
        }

        private void SetStatus(RuntimeView runtime, ViewLifecycleStatus status)
        {
            runtime.status = status;
            if (runtime.entity.TryUnpack<TWorld>(out var entity) &&
                entity.Has<ViewComponent>())
            {
                ref var component = ref entity.Ref<ViewComponent>();
                component.Status = status;
            }

            NotifyStatus(runtime, status);
        }

        private static ViewComponent CreateComponent(
            ViewKey key,
            in ViewRequest request,
            ViewLifecycleStatus status)
        {
            return new ViewComponent
            {
                Key = key,
                ViewId = request.ViewId,
                Layout = request.Placement.Layout,
                Container = request.Placement.Container,
                Queue = request.Queue,
                Owner = request.Owner,
                Status = status,
                Order = request.Order
            };
        }

        private void AttachRelations(
            World<TWorld>.Entity entity,
            in ViewRequest request)
        {
            if (request.Source.TryUnpack<TWorld>(out _))
                entity.Set(new World<TWorld>.Link<ViewSource>(request.Source));

            var parent = request.Placement.ParentEntity;
            if (request.Placement.Mode == ViewPlacementMode.ParentView &&
                _views.TryGetValue(request.Placement.ParentView, out var parentView))
                parent = parentView.entity;

            if (parent.TryUnpack<TWorld>(out _))
                entity.Set(new World<TWorld>.Link<ViewParent>(parent));
        }

        private static ViewLifecycleStatus MapStatus(ViewStatus status)
        {
            return status switch
            {
                ViewStatus.Hidden => ViewLifecycleStatus.Hidden,
                ViewStatus.Shown => ViewLifecycleStatus.Shown,
                ViewStatus.Showing => ViewLifecycleStatus.Showing,
                ViewStatus.Hiding => ViewLifecycleStatus.Hiding,
                ViewStatus.Closed => ViewLifecycleStatus.Closed,
                _ => ViewLifecycleStatus.Opening
            };
        }

        private static bool OwnerIsDead(EntityGID owner)
        {
            return owner != default && !owner.TryUnpack<TWorld>(out _);
        }

        private static bool Matches(RuntimeView runtime, in ViewCloseFilter filter)
        {
            if (filter.All)
                return true;

            if (filter.Owner != default && filter.Owner != runtime.request.Owner)
                return false;

            if (filter.Source != default && filter.Source != runtime.request.Source)
                return false;

            if (!string.IsNullOrEmpty(filter.ViewId) &&
                filter.ViewId != runtime.request.ViewId)
                return false;

            if (!string.IsNullOrEmpty(filter.Layout) &&
                filter.Layout != runtime.request.Placement.Layout)
                return false;

            return filter.Owner != default ||
                   filter.Source != default ||
                   !string.IsNullOrEmpty(filter.ViewId) ||
                   !string.IsNullOrEmpty(filter.Layout);
        }

        private void NotifyStatus(RuntimeView runtime, ViewLifecycleStatus status)
        {
            World<TWorld>.SendEvent(new ViewStatusChangedEvent
            {
                Key = runtime.key,
                Entity = runtime.entity,
                Status = status
            });
        }

        private static void NotifyFailed(ViewKey key, EntityGID entity, string reason)
        {
            World<TWorld>.SendEvent(new ViewOpenFailedEvent
            {
                Key = key,
                Entity = entity,
                Reason = reason
            });
        }

        private void SnapshotKeys()
        {
            _keys.Clear();
            foreach (var key in _views.Keys)
            {
                _keys.Add(key);
            }
        }

        private sealed class RuntimeView
        {
            public ViewKey key;
            public EntityGID entity;
            public ViewRequest request;
            public IView view;
            public IViewModelComponentBinder<TWorld> binder;
            public ViewLifecycleStatus status;
            public bool ownsView;
            public bool ownsEntity;
            public bool containerReserved;
            public long terminalUpdate = -1;
        }

        private struct OpenCompletion
        {
            public ViewKey key;
            public IView view;
            public string error;
            public bool ownsView;
        }
    }
}
