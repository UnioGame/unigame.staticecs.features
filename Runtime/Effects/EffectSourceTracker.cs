using System;
using FFS.Libraries.StaticEcs;

namespace unigame.staticecs.features {
    /// <summary>
    /// Marker component installed on every entity that has applied at least one effect to a
    /// target. Its <c>OnDelete</c> hook walks <see cref="EffectBackRef"/> and dispatches
    /// <see cref="EffectRegistry"/> cleanup so dependants do not need to poll
    /// <c>Source.Status</c> per-tick.
    /// </summary>
    [Serializable]
    public struct EffectSourceTracker : IComponent {
        public void OnDelete<TWorld>(World<TWorld>.Entity self, HookReason reason)
            where TWorld : struct, IWorldType {
            if (reason == HookReason.WorldDestroy) {
                return;
            }

            if (!self.Has<World<TWorld>.Multi<EffectBackRef>>()) {
                return;
            }

            if (!World<TWorld>.HasResource<EffectRegistry>()) {
                return;
            }

            ref var refs = ref self.Ref<World<TWorld>.Multi<EffectBackRef>>();
            if (refs.IsEmpty) {
                return;
            }

            ref var registry = ref World<TWorld>.GetResource<EffectRegistry>();
            EntityGID sourceGid = self.GID;

            for (var i = 0; i < refs.Length; i++) {
                ref var entry = ref refs[i];
                registry.InvokeMask(entry.Mask, sourceGid, entry.Target);
            }
        }
    }
}
