namespace UniGame.StaticEcs.Features
{
    using FFS.Libraries.StaticEcs;

    /// <summary>Links an element entity to the ECS entity of its owning view.</summary>
    public struct UiToolkitElementParent : ILinkType
    {
        public void OnAdd<TWorld>(World<TWorld>.Entity self, EntityGID link)
            where TWorld : struct, IWorldType
        {
            link.TryAddLinkItem<TWorld, UiToolkitChildElements>(self);
        }

        public void OnDelete<TWorld>(World<TWorld>.Entity self, EntityGID link, HookReason reason)
            where TWorld : struct, IWorldType
        {
            if (reason != HookReason.WorldDestroy)
                link.TryDeleteLinkItem<TWorld, UiToolkitChildElements>(self);
        }
    }

    /// <summary>Reverse links from a view entity to its UI Toolkit element entities.</summary>
    public struct UiToolkitChildElements : ILinksType
    {
    }
}
