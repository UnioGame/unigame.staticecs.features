namespace UniGame.StaticEcs.Features
{
    using System;
    using FFS.Libraries.StaticEcs;
    using UniGame.StaticEcs.Unity;
    using UniGame.ViewSystem.Runtime;

    /// <summary>Provides command operations for ECS-managed View System views.</summary>
    public static class ViewOperations
    {
        /// <summary>Requests a view on the supplied Main-world entity.</summary>
        public static bool TryOpenOn(
            World<Main>.Entity entity,
            in ViewRequest request,
            out ViewKey key)
        {
            return TryOpenOn<Main>(entity, request, out key);
        }

        /// <summary>Requests a Main-world view using a generated View System ID.</summary>
        public static bool TryOpenOn<TViewId>(
            World<Main>.Entity entity,
            TViewId viewId,
            in ViewRequest request,
            out ViewKey key)
            where TViewId : struct, Enum
        {
            if (!TryApplyViewId(viewId, request, out var typedRequest))
            {
                key = ViewKey.Invalid;
                return false;
            }

            return TryOpenOnCore<Main>(entity, typedRequest, out key);
        }

        /// <summary>Requests a view on the supplied entity.</summary>
        public static bool TryOpenOn<TWorld>(
            World<TWorld>.Entity entity,
            in ViewRequest request,
            out ViewKey key)
            where TWorld : struct, IWorldType
        {
            return TryOpenOnCore<TWorld>(entity, request, out key);
        }

        private static bool TryOpenOnCore<TWorld>(
            World<TWorld>.Entity entity,
            in ViewRequest request,
            out ViewKey key)
            where TWorld : struct, IWorldType
        {
            key = ViewKey.Invalid;
            if (!IsValid(request) || entity.Has<ViewComponent>() ||
                !World<TWorld>.HasResource<ViewKeySequenceResource<TWorld>>())
                return false;

            ref var sequence = ref World<TWorld>.GetResource<ViewKeySequenceResource<TWorld>>();
            var next = sequence.Next();
            if (!World<TWorld>.SendEvent(new OpenViewOnEntityEvent
                {
                    Key = next,
                    Entity = entity.GID,
                    Request = ViewRequest.WithDefaultPlacement(request)
                }))
                return false;

            key = next;
            return true;
        }

        /// <summary>Requests a view using a generated View System ID.</summary>
        public static bool TryOpenOn<TWorld, TViewId>(
            World<TWorld>.Entity entity,
            TViewId viewId,
            in ViewRequest request,
            out ViewKey key)
            where TWorld : struct, IWorldType
            where TViewId : struct, Enum
        {
            if (!TryApplyViewId(viewId, request, out var typedRequest))
            {
                key = ViewKey.Invalid;
                return false;
            }

            return TryOpenOnCore<TWorld>(entity, typedRequest, out key);
        }

        /// <summary>Requests a Main-world view by its registered class type.</summary>
        public static bool TryOpenOnByType<TView>(
            World<Main>.Entity entity,
            out ViewKey key)
            where TView : IView =>
            TryOpenOnCore<Main>(entity, ViewRequest.For<TView>(), out key);

        /// <summary>Requests a visible Main-world view on an entity with the selected placement.</summary>
        public static bool TryOpenOnByType<TView>(
            World<Main>.Entity entity,
            ViewPlacement placement,
            out ViewKey key)
            where TView : IView =>
            TryOpenOnByType<Main, TView>(entity, placement, out key);

        /// <summary>Requests a Main-world view by type, preserving the supplied request options.</summary>
        public static bool TryOpenOnByType<TView>(
            World<Main>.Entity entity,
            in ViewRequest request,
            out ViewKey key)
            where TView : IView =>
            TryOpenOnCore<Main>(entity, ViewRequest.BindViewType<TView>(request), out key);

        /// <summary>Requests a view on an entity by its registered class type.</summary>
        public static bool TryOpenOnByType<TWorld, TView>(
            World<TWorld>.Entity entity,
            out ViewKey key)
            where TWorld : struct, IWorldType
            where TView : IView =>
            TryOpenOnCore<TWorld>(entity, ViewRequest.For<TView>(), out key);

        /// <summary>Requests a visible view on an entity with the selected placement.</summary>
        public static bool TryOpenOnByType<TWorld, TView>(
            World<TWorld>.Entity entity,
            ViewPlacement placement,
            out ViewKey key)
            where TWorld : struct, IWorldType
            where TView : IView
        {
            var request = ViewRequest.For<TView>();
            request.Placement = placement;
            return TryOpenOnCore<TWorld>(
                entity, ViewRequest.BindViewType<TView>(request), out key);
        }

        /// <summary>Requests a view on an entity by type, preserving the supplied request options.</summary>
        public static bool TryOpenOnByType<TWorld, TView>(
            World<TWorld>.Entity entity,
            in ViewRequest request,
            out ViewKey key)
            where TWorld : struct, IWorldType
            where TView : IView =>
            TryOpenOnCore<TWorld>(entity, ViewRequest.BindViewType<TView>(request), out key);

        /// <summary>Requests a view on a new Main-world ECS entity.</summary>
        public static bool TryOpenNew(in ViewRequest request, out ViewKey key)
        {
            return TryOpenNew<Main>(request, out key);
        }

        /// <summary>Requests a new Main-world view using a generated View System ID.</summary>
        public static bool TryOpenNew<TViewId>(
            TViewId viewId,
            in ViewRequest request,
            out ViewKey key)
            where TViewId : struct, Enum
        {
            if (!TryApplyViewId(viewId, request, out var typedRequest))
            {
                key = ViewKey.Invalid;
                return false;
            }

            return TryOpenNewCore<Main>(typedRequest, out key);
        }

        /// <summary>Requests a view on a new ECS entity.</summary>
        public static bool TryOpenNew<TWorld>(in ViewRequest request, out ViewKey key)
            where TWorld : struct, IWorldType
        {
            return TryOpenNewCore<TWorld>(request, out key);
        }

        private static bool TryOpenNewCore<TWorld>(
            in ViewRequest request,
            out ViewKey key)
            where TWorld : struct, IWorldType
        {
            key = ViewKey.Invalid;
            if (!IsValid(request) ||
                !World<TWorld>.HasResource<ViewKeySequenceResource<TWorld>>())
                return false;

            ref var sequence = ref World<TWorld>.GetResource<ViewKeySequenceResource<TWorld>>();
            var next = sequence.Next();
            if (!World<TWorld>.SendEvent(new OpenViewEvent
                {
                    Key = next,
                    Request = ViewRequest.WithDefaultPlacement(request)
                }))
                return false;

            key = next;
            return true;
        }

        /// <summary>Requests a view in a selected world using a generated View System ID.</summary>
        public static bool TryOpenNew<TWorld, TViewId>(
            TViewId viewId,
            in ViewRequest request,
            out ViewKey key)
            where TWorld : struct, IWorldType
            where TViewId : struct, Enum
        {
            if (!TryApplyViewId(viewId, request, out var typedRequest))
            {
                key = ViewKey.Invalid;
                return false;
            }

            return TryOpenNewCore<TWorld>(typedRequest, out key);
        }

        /// <summary>Requests a visible Main-world view by its registered class type.</summary>
        public static bool TryOpenNewByType<TView>(out ViewKey key)
            where TView : IView =>
            TryOpenNewCore<Main>(ViewRequest.For<TView>(), out key);

        /// <summary>Requests a visible Main-world view with the selected placement.</summary>
        public static bool TryOpenNewByType<TView>(
            ViewPlacement placement,
            out ViewKey key)
            where TView : IView =>
            TryOpenNewByType<Main, TView>(placement, out key);

        /// <summary>Requests a Main-world view by type, preserving the supplied request options.</summary>
        public static bool TryOpenNewByType<TView>(
            in ViewRequest request,
            out ViewKey key)
            where TView : IView =>
            TryOpenNewCore<Main>(ViewRequest.BindViewType<TView>(request), out key);

        /// <summary>Requests a visible view in the selected world by its registered class type.</summary>
        public static bool TryOpenNewByType<TWorld, TView>(out ViewKey key)
            where TWorld : struct, IWorldType
            where TView : IView =>
            TryOpenNewCore<TWorld>(ViewRequest.For<TView>(), out key);

        /// <summary>Requests a visible view in the selected world with the selected placement.</summary>
        public static bool TryOpenNewByType<TWorld, TView>(
            ViewPlacement placement,
            out ViewKey key)
            where TWorld : struct, IWorldType
            where TView : IView
        {
            var request = ViewRequest.For<TView>();
            request.Placement = placement;
            return TryOpenNewCore<TWorld>(
                ViewRequest.BindViewType<TView>(request), out key);
        }

        /// <summary>Requests a view in the selected world by type, preserving the supplied request options.</summary>
        public static bool TryOpenNewByType<TWorld, TView>(
            in ViewRequest request,
            out ViewKey key)
            where TWorld : struct, IWorldType
            where TView : IView =>
            TryOpenNewCore<TWorld>(ViewRequest.BindViewType<TView>(request), out key);

        /// <summary>Binds an existing Main-world prefab view to its entity.</summary>
        public static bool TryBindExistingOn(
            World<Main>.Entity entity,
            IView view,
            in ViewRequest request,
            out ViewKey key)
        {
            return TryBindExistingOn<Main>(entity, view, request, out key);
        }

        /// <summary>Binds an existing Main-world view using a generated View System ID.</summary>
        public static bool TryBindExistingOn<TViewId>(
            World<Main>.Entity entity,
            IView view,
            TViewId viewId,
            in ViewRequest request,
            out ViewKey key)
            where TViewId : struct, Enum
        {
            if (!TryApplyViewId(viewId, request, out var typedRequest))
            {
                key = ViewKey.Invalid;
                return false;
            }

            return TryBindExistingOnCore<Main>(entity, view, typedRequest, out key);
        }

        /// <summary>Binds an existing prefab view to its entity.</summary>
        public static bool TryBindExistingOn<TWorld>(
            World<TWorld>.Entity entity,
            IView view,
            in ViewRequest request,
            out ViewKey key)
            where TWorld : struct, IWorldType
        {
            return TryBindExistingOnCore<TWorld>(entity, view, request, out key);
        }

        private static bool TryBindExistingOnCore<TWorld>(
            World<TWorld>.Entity entity,
            IView view,
            in ViewRequest request,
            out ViewKey key)
            where TWorld : struct, IWorldType
        {
            key = ViewKey.Invalid;
            if (view == null || !IsValid(request) || entity.Has<ViewComponent>() ||
                request.Placement.Mode != ViewPlacementMode.Existing ||
                !World<TWorld>.HasResource<ViewKeySequenceResource<TWorld>>())
                return false;

            ref var sequence = ref World<TWorld>.GetResource<ViewKeySequenceResource<TWorld>>();
            var next = sequence.Next();
            if (!World<TWorld>.SendEvent(new BindExistingViewEvent
                {
                    Key = next,
                    Entity = entity.GID,
                    Request = request,
                    View = view
                }))
                return false;

            key = next;
            return true;
        }

        /// <summary>Binds an existing view using a generated View System ID.</summary>
        public static bool TryBindExistingOn<TWorld, TViewId>(
            World<TWorld>.Entity entity,
            IView view,
            TViewId viewId,
            in ViewRequest request,
            out ViewKey key)
            where TWorld : struct, IWorldType
            where TViewId : struct, Enum
        {
            if (!TryApplyViewId(viewId, request, out var typedRequest))
            {
                key = ViewKey.Invalid;
                return false;
            }

            return TryBindExistingOnCore<TWorld>(entity, view, typedRequest, out key);
        }

        /// <summary>Requests showing a Main-world view.</summary>
        public static bool Show(ViewKey key) => Show<Main>(key);

        /// <summary>Requests showing a view.</summary>
        public static bool Show<TWorld>(ViewKey key)
            where TWorld : struct, IWorldType =>
            key.IsValid && World<TWorld>.SendEvent(new SetViewVisibilityEvent
            {
                Key = key,
                Visible = true
            });

        /// <summary>Requests hiding a Main-world view.</summary>
        public static bool Hide(ViewKey key) => Hide<Main>(key);

        /// <summary>Requests hiding a view.</summary>
        public static bool Hide<TWorld>(ViewKey key)
            where TWorld : struct, IWorldType =>
            key.IsValid && World<TWorld>.SendEvent(new SetViewVisibilityEvent
            {
                Key = key,
                Visible = false
            });

        /// <summary>Requests closing a Main-world view.</summary>
        public static bool Close(ViewKey key) => Close<Main>(key);

        /// <summary>Requests closing a view.</summary>
        public static bool Close<TWorld>(ViewKey key)
            where TWorld : struct, IWorldType =>
            key.IsValid && World<TWorld>.SendEvent(new CloseViewEvent { Key = key });

        /// <summary>Requests closing Main-world views matching a filter.</summary>
        public static bool CloseAll(in ViewCloseFilter filter) => CloseAll<Main>(filter);

        /// <summary>Requests closing views matching a filter.</summary>
        public static bool CloseAll<TWorld>(in ViewCloseFilter filter)
            where TWorld : struct, IWorldType =>
            World<TWorld>.SendEvent(new CloseViewsEvent { Filter = filter });

        /// <summary>Requests closing every Main-world view of the exact class type.</summary>
        public static bool CloseAllByType<TView>()
            where TView : IView =>
            CloseAllByType<Main, TView>();

        /// <summary>Requests closing every view of the exact class type in the selected world.</summary>
        public static bool CloseAllByType<TWorld, TView>()
            where TWorld : struct, IWorldType
            where TView : IView =>
            CloseAll<TWorld>(new ViewCloseFilter
            {
                ViewType = typeof(TView).AssemblyQualifiedName
            });

        /// <summary>Requests closing all views owned by a Main-world entity.</summary>
        public static bool CloseOwned(EntityGID owner) => CloseOwned<Main>(owner);

        /// <summary>Requests closing all views owned by an entity.</summary>
        public static bool CloseOwned<TWorld>(EntityGID owner)
            where TWorld : struct, IWorldType =>
            CloseAll<TWorld>(new ViewCloseFilter { Owner = owner });

        private static bool IsValid(in ViewRequest request)
        {
            return !string.IsNullOrWhiteSpace(request.ViewId);
        }

        private static bool TryApplyViewId<TViewId>(
            TViewId viewId,
            in ViewRequest request,
            out ViewRequest typedRequest)
            where TViewId : struct, Enum
        {
            if (!ViewSystemIdResolver.TryResolve(viewId, out var value))
            {
                typedRequest = default;
                return false;
            }

            typedRequest = request;
            typedRequest.ViewId = value;
            return true;
        }
    }
}
