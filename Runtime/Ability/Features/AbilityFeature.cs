using FFS.Libraries.StaticEcs;
using unigame.staticecs;

namespace unigame.staticecs.features {
    /// <summary>
    /// Wires the ability slice: registers <see cref="AbilityCastComponent"/>,
    /// <see cref="AbilityRosterEntry"/> and <see cref="CooldownEntry"/> multi-components, the
    /// lifecycle events, the world's <see cref="AbilityRegistry{TWorld}"/> resource, and adds
    /// <see cref="AbilityCastSystem{TWorld}"/> + <see cref="AbilityTickSystem{TWorld}"/> to the
    /// update group. Pair with <see cref="TargetSelectionFeature{TWorld}"/> for spatial queries.
    /// </summary>
    public class AbilityFeature<TWorld> :
        StaticEcsFeature<TWorld>,
        IStaticEcsSystemsFeature<TWorld, StaticEcsUpdateSystems>
        where TWorld : struct, IWorldType {
        public const short DefaultCastOrder = 150;
        public const short DefaultTickOrder = 160;

        private readonly short _castOrder;
        private readonly short _tickOrder;
        private readonly bool _registerSystems;

        public AbilityFeature(
            bool registerSystems = true,
            short castOrder = DefaultCastOrder,
            short tickOrder = DefaultTickOrder) {
            _registerSystems = registerSystems;
            _castOrder = castOrder;
            _tickOrder = tickOrder;
        }

        public override void RegisterTypes(World<TWorld>.TypeRegistrar types) {
            types
                .Component<AbilityCastComponent>()
                .Multi<AbilityRosterEntry>()
                .Multi<CooldownEntry>()
                .Event<CastAbilityEvent>()
                .Event<AbilityStateChangedEvent>()
                .Event<CooldownReadyEvent>();

            if (!World<TWorld>.HasResource<AbilityRegistry<TWorld>>()) {
                World<TWorld>.SetResource(new AbilityRegistry<TWorld>());
            }
        }

        public void RegisterSystems(StaticEcsSystemsBuilder<TWorld, StaticEcsUpdateSystems> systems) {
            if (!_registerSystems) {
                return;
            }
            systems.Add(new AbilityCastSystem<TWorld>(), _castOrder);
            systems.Add(new AbilityTickSystem<TWorld>(), _tickOrder);
        }
    }
}
