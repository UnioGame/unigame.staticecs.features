namespace UniGame.StaticEcs.Features
{
    using System;
    using System.Buffers;
    using FFS.Libraries.StaticEcs;

    /// <summary>
    /// Marker component installed on every entity that has applied at least one effect to a
    /// target. Its <c>OnDelete</c> hook walks <see cref="EffectTargetComponent"/> and dispatches
    /// <see cref="EffectRegistry"/> cleanup so dependants do not need to poll
    /// <c>Source.Status</c> per-tick.
    /// </summary>
    [Serializable]
    public struct EffectTrackerComponent : IComponent
    {
        /// <summary>Handles source-entity deletion and dispatches effect cleanup.</summary>
        public void OnDelete<TWorld>(World<TWorld>.Entity self, HookReason reason)
            where TWorld : struct, IWorldType
        {
            if (reason == HookReason.WorldDestroy)
                return;

            if (!self.Has<World<TWorld>.Multi<EffectTargetComponent>>())
                return;

            if (!World<TWorld>.HasResource<EffectRegistry>())
                return;

            ref var refs = ref self.Ref<World<TWorld>.Multi<EffectTargetComponent>>();
            if (refs.IsEmpty)
                return;

            ref var registry = ref World<TWorld>.GetResource<EffectRegistry>();
            EntityGID sourceGid = self.GID;

            int count = refs.Length;
            EffectTargetComponent[] rented = null;
            Span<EffectTargetComponent> snapshot = count <= 64
                ? stackalloc EffectTargetComponent[count]
                : (rented = ArrayPool<EffectTargetComponent>.Shared.Rent(count));
            try
            {
                snapshot = snapshot[..count];
                for (var i = 0; i < count; i++)
                    snapshot[i] = refs[i];

                for (var i = 0; i < count; i++)
                {
                    var entry = snapshot[i];
                    registry.InvokeMask(entry.Mask, sourceGid, entry.Target);
                }
            }
            finally
            {
                if (rented != null)
                    ArrayPool<EffectTargetComponent>.Shared.Return(rented);
            }
        }
    }
}
