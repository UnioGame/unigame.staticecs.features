namespace UniGame.StaticEcs.Features
{
    using System;
    using FFS.Libraries.StaticEcs;
    using Modifiers;

    /// <summary>Removes source-owned modifiers from tracked targets when the source is deleted.</summary>
    [Serializable]
    public struct ModifierTrackerComponent : IComponent
    {
        /// <summary>Handles source-entity deletion and dispatches modifier cleanup.</summary>
        public void OnDelete<TWorld>(World<TWorld>.Entity self, HookReason reason)
            where TWorld : struct, IWorldType
        {
            if (reason == HookReason.WorldDestroy)
                return;

            if (!self.Has<World<TWorld>.Multi<ModifierTargetComponent>>())
                return;

            if (!World<TWorld>.HasResource<ModifierRegistry>())
                return;

            ref var refs = ref self.Ref<World<TWorld>.Multi<ModifierTargetComponent>>();
            if (refs.IsEmpty)
                return;

            ref var registry = ref World<TWorld>.GetResource<ModifierRegistry>();
            EntityGID sourceGid = self.GID;

            for (var i = 0; i < refs.Length; i++)
            {
                ref var entry = ref refs[i];
                registry.InvokeMask((ulong)entry.StatMask, sourceGid, entry.Target);
            }
        }
    }
}
