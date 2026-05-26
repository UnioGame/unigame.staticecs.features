using FFS.Libraries.StaticEcs;
using unigame.staticecs;

namespace unigame.staticecs.features {
    /// <summary>
    /// Wires the ability slice for a world: registers cast-entity components / tags, the
    /// caster-side <see cref="AbilityActiveCastRef"/> + <see cref="AbilityChannelCastRef"/>
    /// roster, the lifecycle events, the world-scoped <see cref="AbilityRegistry{TWorld}"/>
    /// and <see cref="AbilityStepActivatorRegistry{TWorld}"/> resources, and the smoke-pipeline
    /// systems (<see cref="AbilityCastSystem{TWorld}"/>, <see cref="AbilityWaitSystem{TWorld}"/>,
    /// <see cref="AbilityStepProgressionSystem{TWorld}"/>).
    ///
    /// Registers built-in leaf activators for the runtime ability pipeline.
    /// </summary>
    public class AbilityFeature<TWorld> :
        StaticEcsFeature<TWorld>,
        IStaticEcsSystemsFeature<TWorld, StaticEcsUpdateSystems>
        where TWorld : struct, IWorldType {
        public const short DefaultCastOrder = 150;
        public const short DefaultWaitOrder = 155;
        public const short DefaultProgressionOrder = 160;

        private readonly short _castOrder;
        private readonly short _waitOrder;
        private readonly short _progressionOrder;
        private readonly bool _registerSystems;

        public AbilityFeature(
            bool registerSystems = true,
            short castOrder = DefaultCastOrder,
            short waitOrder = DefaultWaitOrder,
            short progressionOrder = DefaultProgressionOrder) {
            _registerSystems = registerSystems;
            _castOrder = castOrder;
            _waitOrder = waitOrder;
            _progressionOrder = progressionOrder;
        }

        public override void RegisterTypes(World<TWorld>.TypeRegistrar types) {
            types
                .Component<AbilityCastRuntimeComponent>()
                .Component<AbilityCastOwnerRef>()
                .Component<AbilityCastParentRef>()
                .Component<AbilityActiveCastRef>()
                .Component<AbilityCurrentLeaf>()
                .Component<AbilityStepLastStatus>()
                .Component<AbilityWaitState>()
                .Component<AbilityInlineRootConfig>()
                .Multi<AbilityRosterEntry>()
                .Multi<CooldownEntry>()
                .Multi<AbilityChannelCastRef>()
                .Multi<AbilityStackFrame>()
                .Multi<AbilityActiveStepEntry>()
                .Multi<AbilityAoeBufferEntry>()
                .Multi<AbilityParallelBranchEntry>()
                .Tag<AbilityStepReadyTag>()
                .Tag<AbilityChannelCastTag>()
                .Tag<AbilityDetachedSubcastTag>()
                .Tag<AbilityBranchSubcastTag>()
                .Tag<AbilityParallelWaitingTag>()
                .Event<CastAbilityEvent>()
                .Event<AbilityStartedEvent>()
                .Event<AbilityCompletedEvent>()
                .Event<AbilityBranchCompletedEvent>()
                .Event<AbilityStepStartedEvent>()
                .Event<AbilityStepCompletedEvent>()
                .Event<CooldownReadyEvent>();

            if (!World<TWorld>.HasResource<AbilityRegistry<TWorld>>()) {
                World<TWorld>.SetResource(new AbilityRegistry<TWorld>());
            }

            if (!World<TWorld>.HasResource<AbilityEffectDispatchRegistry<TWorld>>()) {
                World<TWorld>.SetResource(new AbilityEffectDispatchRegistry<TWorld>());
            }

            if (!World<TWorld>.HasResource<IAbilityRng<TWorld>>()) {
                World<TWorld>.SetResource<IAbilityRng<TWorld>>(new UnityAbilityRng<TWorld>());
            }

            if (!World<TWorld>.HasResource<AbilityStepActivatorRegistry<TWorld>>()) {
                var activators = new AbilityStepActivatorRegistry<TWorld>();
                activators.Register<WaitStepConfig>(new WaitStepActivator<TWorld>());
                activators.Register<ApplyDamageStepConfig>(new ApplyDamageStepActivator<TWorld>());
                activators.Register<ApplyEffectStepConfig>(new ApplyEffectStepActivator<TWorld>());
                activators.Register<AoeQueryStepConfig>(new AoeQueryStepActivator<TWorld>());
                activators.Register<SetPrimaryTargetFromAoeStepConfig>(new SetPrimaryTargetFromAoeStepActivator<TWorld>());
                World<TWorld>.SetResource(activators);
            }
        }

        public void RegisterSystems(StaticEcsSystemsBuilder<TWorld, StaticEcsUpdateSystems> systems) {
            if (!_registerSystems) {
                return;
            }
            systems.Add(new AbilityCastSystem<TWorld>(), _castOrder);
            systems.Add(new AbilityWaitSystem<TWorld>(), _waitOrder);
            systems.Add(new AbilityStepProgressionSystem<TWorld>(), _progressionOrder);
        }
    }
}
