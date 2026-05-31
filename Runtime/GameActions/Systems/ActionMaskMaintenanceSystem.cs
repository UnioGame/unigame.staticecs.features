using FFS.Libraries.StaticEcs;

namespace unigame.staticecs.features {
    /// <summary>
    /// Responds to <see cref="StunChangedEvent"/>s and keeps <see cref="ActionMaskComponent"/>
    /// in sync with stun state:
    /// <list type="bullet">
    ///   <item>Stun becomes active (<see cref="StunChangedEvent.BecameActive"/>) → <c>Bits = 0</c>.</item>
    ///   <item>Stun clears (<see cref="StunChangedEvent.BecameInactive"/>) → <c>Bits = uint.MaxValue</c>.</item>
    /// </list>
    /// Entities without <see cref="ActionMaskComponent"/> are skipped.
    /// <para>
    /// <b>Prerequisite:</b> <see cref="StunFeature{TWorld}"/> must be registered before this
    /// system is added to the update group.
    /// </para>
    /// </summary>
    public sealed class ActionMaskMaintenanceSystem<TWorld> : ISystem
        where TWorld : struct, IWorldType {
        private EventReceiver<TWorld, StunChangedEvent> _receiver;

        /// <inheritdoc/>
        public void Init() {
            _receiver = World<TWorld>.RegisterEventReceiver<StunChangedEvent>();
        }

        /// <inheritdoc/>
        public void Update() {
            foreach (var e in _receiver) {
                ref readonly var ev = ref e.Value;
                if (!ev.BecameActive && !ev.BecameInactive) {
                    continue;
                }

                if (!ev.Target.TryUnpack<TWorld>(out var entity)) {
                    continue;
                }

                if (!entity.Has<ActionMaskComponent>()) {
                    continue;
                }

                entity.Mut<ActionMaskComponent>().Bits = ev.BecameActive ? 0u : uint.MaxValue;
            }
        }

        /// <inheritdoc/>
        public void Destroy() {
            World<TWorld>.DeleteEventReceiver(ref _receiver);
        }
    }
}
