using System;
using FFS.Libraries.StaticEcs;


namespace UniGame.StaticEcs.Features
{
    using Modifiers;

    [Serializable]
    public struct ModifierSourceTracker : IComponent
    {
        public void OnDelete<TWorld>(World<TWorld>.Entity self, HookReason reason)
            where TWorld : struct, IWorldType
        {
            if (reason == HookReason.WorldDestroy)
            {
                return;
            }

            if (!self.Has<World<TWorld>.Multi<ModifierBackRef>>())
            {
                return;
            }

            if (!World<TWorld>.HasResource<ModifierRegistry>())
            {
                return;
            }

            ref var refs = ref self.Ref<World<TWorld>.Multi<ModifierBackRef>>();
            if (refs.IsEmpty)
            {
                return;
            }

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