using FFS.Libraries.StaticEcs;

namespace UniGame.StaticEcs.Features {
    /// <summary>
    /// Receives <see cref="CastAbilityEvent"/>s, re-validates ability-internal invariants
    /// (the requesting business layer may have queued the event in a previous tick — state
    /// can have shifted), spawns a cast-entity with runtime + owner components, sets
    /// <see cref="AbilityActiveCastRef"/> on the caster, and arms
    /// <see cref="AbilityStepProgressionSystem{TWorld}"/> by setting
    /// <see cref="AbilityStepReadyTag"/> with status Success — the progression system then
    /// descends into the root step in the same frame (it is scheduled right after this system).
    /// </summary>
    public sealed class AbilityCastSystem<TWorld> : ISystem
        where TWorld : struct, IWorldType {
        private EventReceiver<TWorld, CastAbilityEvent> _receiver;

        public void Init() {
            _receiver = World<TWorld>.RegisterEventReceiver<CastAbilityEvent>();
        }

        public void Update() {
            if (!World<TWorld>.HasResource<AbilityRegistry<TWorld>>()) {
                foreach (var _ in _receiver) { }
                return;
            }

            var registry = World<TWorld>.GetResource<AbilityRegistry<TWorld>>();

            foreach (var e in _receiver) {
                var ev = e.Value;
                if (!ev.Caster.TryUnpack<TWorld>(out var caster)) {
                    continue;
                }
                if (caster.Has<AbilityActiveCastRef>()) {
                    continue;
                }
                if (!AbilityOperations.HasAbility<TWorld>(ev.Caster, ev.AbilityId)) {
                    continue;
                }
                if (!registry.Contains(ev.AbilityId)) {
                    continue;
                }

                AbilityCastFactory.SpawnRoot<TWorld>(ev.Caster, ev.AbilityId, ev.Target);
            }
        }

        public void Destroy() {
            World<TWorld>.DeleteEventReceiver(ref _receiver);
        }
    }
}
