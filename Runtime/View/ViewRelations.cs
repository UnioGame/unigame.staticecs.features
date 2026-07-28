namespace UniGame.StaticEcs.Features
{
    using FFS.Libraries.StaticEcs;

    /// <summary>Defines the single gameplay source relation of a view entity.</summary>
    public struct ViewSource : ILinkType
    {
        /// <inheritdoc />
        public void OnAdd<TWorld>(World<TWorld>.Entity self, EntityGID link)
            where TWorld : struct, IWorldType
        {
            link.TryAddLinkItem<TWorld, SourceViews>(self);
        }

        /// <inheritdoc />
        public void OnDelete<TWorld>(
            World<TWorld>.Entity self,
            EntityGID link,
            HookReason reason)
            where TWorld : struct, IWorldType
        {
            if (reason != HookReason.WorldDestroy)
                link.TryDeleteLinkItem<TWorld, SourceViews>(self);
        }
    }

    /// <summary>Defines the collection of views associated with a gameplay source.</summary>
    public struct SourceViews : ILinksType
    {
    }

    /// <summary>Defines the visual parent relation of a view entity.</summary>
    public struct ViewParent : ILinkType
    {
        /// <inheritdoc />
        public void OnAdd<TWorld>(World<TWorld>.Entity self, EntityGID link)
            where TWorld : struct, IWorldType
        {
            link.TryAddLinkItem<TWorld, ViewChildren>(self);
        }

        /// <inheritdoc />
        public void OnDelete<TWorld>(
            World<TWorld>.Entity self,
            EntityGID link,
            HookReason reason)
            where TWorld : struct, IWorldType
        {
            if (reason != HookReason.WorldDestroy)
                link.TryDeleteLinkItem<TWorld, ViewChildren>(self);
        }
    }

    /// <summary>Defines the collection of visual child views.</summary>
    public struct ViewChildren : ILinksType
    {
    }
}
