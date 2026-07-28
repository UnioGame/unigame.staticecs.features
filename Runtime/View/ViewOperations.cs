namespace UniGame.StaticEcs.Features
{
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

        /// <summary>Requests a view on the supplied entity.</summary>
        public static bool TryOpenOn<TWorld>(
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
                    Request = request
                }))
                return false;

            key = next;
            return true;
        }

        /// <summary>Requests a view on a new Main-world ECS entity.</summary>
        public static bool TryOpenNew(in ViewRequest request, out ViewKey key)
        {
            return TryOpenNew<Main>(request, out key);
        }

        /// <summary>Requests a view on a new ECS entity.</summary>
        public static bool TryOpenNew<TWorld>(in ViewRequest request, out ViewKey key)
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
                    Request = request
                }))
                return false;

            key = next;
            return true;
        }

        /// <summary>Binds an existing Main-world prefab view to its entity.</summary>
        public static bool TryBindExistingOn(
            World<Main>.Entity entity,
            IView view,
            in ViewRequest request,
            out ViewKey key)
        {
            return TryBindExistingOn<Main>(entity, view, request, out key);
        }

        /// <summary>Binds an existing prefab view to its entity.</summary>
        public static bool TryBindExistingOn<TWorld>(
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
    }
}
