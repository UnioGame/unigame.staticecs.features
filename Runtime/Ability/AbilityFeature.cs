namespace UniGame.StaticEcs.Features
{
    using System.Threading;
    using Cysharp.Threading.Tasks;
    using FFS.Libraries.StaticEcs;

    /// <summary>
    /// Wires the ability slice for a world: registers cast-entity components / tags, the
    /// caster-side <see cref="AbilityActiveCastComponent"/> + <see cref="AbilityChannelCastComponent"/>
    /// roster, the lifecycle events, the world-scoped <see cref="AbilityRegistry{TWorld}"/>
    /// and <see cref="AbilityStepActivatorRegistry{TWorld}"/> resources, and the smoke-pipeline
    /// systems (<see cref="AbilityCastSystem{TWorld}"/>, <see cref="AbilityWaitSystem{TWorld}"/>,
    /// <see cref="AbilityStepProgressionSystem{TWorld}"/>).
    ///
    /// Registers built-in leaf activators for the runtime ability pipeline.
    /// </summary>
    public class AbilityFeature<TWorld>
        : StaticEcsFeature<TWorld>,
            IStaticEcsSystemsFeature<TWorld, StaticEcsUpdateSystems>
        where TWorld : struct, IWorldType
    {
        public const short DefaultCastOrder = 150;
        public const short DefaultWaitOrder = 155;
        public const short DefaultProgressionOrder = 160;

        /// <summary>Whether the standard ability systems are installed.</summary>
        public bool registerSystems = true;

        /// <summary>Execution order of cast processing.</summary>
        public short castOrder = DefaultCastOrder;

        /// <summary>Execution order of wait processing.</summary>
        public short waitOrder = DefaultWaitOrder;

        /// <summary>Execution order of step progression.</summary>
        public short progressionOrder = DefaultProgressionOrder;

        public AbilityFeature(
            bool registerSystems = true,
            short castOrder = DefaultCastOrder,
            short waitOrder = DefaultWaitOrder,
            short progressionOrder = DefaultProgressionOrder
        )
        {
            this.registerSystems = registerSystems;
            this.castOrder = castOrder;
            this.waitOrder = waitOrder;
            this.progressionOrder = progressionOrder;
        }

        public override void RegisterTypes(World<TWorld>.TypeRegistrar types)
        {
            types
                .Component<AbilityCastComponent>()
                .Component<AbilityCastOwnerComponent>()
                .Component<AbilityParentCastComponent>()
                .Component<AbilityActiveCastComponent>()
                .Component<AbilityCurrentStepComponent>()
                .Component<AbilityStepStatusComponent>()
                .Component<AbilityWaitComponent>()
                .Component<AbilityRootComponent>()
                .Multi<AbilitySlotComponent>()
                .Multi<CooldownComponent>()
                .Multi<AbilityChannelCastComponent>()
                .Multi<AbilityStackComponent>()
                .Multi<AbilityActiveStepComponent>()
                .Multi<AbilityAoeTargetComponent>()
                .Multi<AbilityBranchComponent>()
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

            if (!World<TWorld>.HasResource<AbilityRegistry<TWorld>>())
            {
                World<TWorld>.SetResource(new AbilityRegistry<TWorld>());
            }

            if (!World<TWorld>.HasResource<AbilityEffectDispatchRegistry<TWorld>>())
            {
                World<TWorld>.SetResource(new AbilityEffectDispatchRegistry<TWorld>());
            }

            if (!World<TWorld>.HasResource<IAbilityRng<TWorld>>())
            {
                World<TWorld>.SetResource<IAbilityRng<TWorld>>(new UnityAbilityRng<TWorld>());
            }

            if (!World<TWorld>.HasResource<AbilityStepActivatorRegistry<TWorld>>())
            {
                var activators = new AbilityStepActivatorRegistry<TWorld>();
                activators.Register<WaitStepConfig>(new WaitStepActivator<TWorld>());
                activators.Register<ApplyDamageStepConfig>(new ApplyDamageStepActivator<TWorld>());
                activators.Register<ApplyEffectStepConfig>(new ApplyEffectStepActivator<TWorld>());
                activators.Register<AoeQueryStepConfig>(new AoeQueryStepActivator<TWorld>());
                activators.Register<SetPrimaryTargetFromAoeStepConfig>(
                    new SetPrimaryTargetFromAoeStepActivator<TWorld>()
                );
                World<TWorld>.SetResource(activators);
            }
        }

        public UniTask RegisterSystemsAsync(
            StaticEcsSystemsBuilder<TWorld, StaticEcsUpdateSystems> systems,
            CancellationToken cancellationToken
        )
        {
            if (!registerSystems)
            {
                return UniTask.CompletedTask;
            }
            systems.Add(new AbilityCastSystem<TWorld>(), castOrder);
            systems.Add(new AbilityWaitSystem<TWorld>(), waitOrder);
            systems.Add(new AbilityStepProgressionSystem<TWorld>(), progressionOrder);
            return UniTask.CompletedTask;
        }
    }
}
