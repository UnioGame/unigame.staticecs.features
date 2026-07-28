namespace UniGame.StaticEcs.Features
{
    using Cysharp.Threading.Tasks;
    using FFS.Libraries.StaticEcs;
    using UniGame.Core.Runtime;

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
    public class AbilityFeature<TWorld> : StaticEcsFeature<TWorld>
        where TWorld : struct, IWorldType
    {
        public const short DefaultCastOrder = 150;
        public const short DefaultWaitOrder = 155;
        public const short DefaultProgressionOrder = 160;

        /// <summary>Whether the standard ability systems are installed.</summary>
        public bool registerSystems = true;

        /// <summary>Execution order of cast request processing.</summary>
        public short castOrder = DefaultCastOrder;

        /// <summary>Execution order of wait processing.</summary>
        public short waitOrder = DefaultWaitOrder;

        /// <summary>Execution order of step progression.</summary>
        public short progressionOrder = DefaultProgressionOrder;

        /// <inheritdoc />
        public override UniTask InitializeAsync(ILifeTime lifeTime)
        {
            if (!World<TWorld>.HasResource<AbilityConfig>())
            {
                var configuration = new AbilityConfig
                {
                    RegisterSystems = registerSystems,
                    CastOrder = castOrder,
                    WaitOrder = waitOrder,
                    ProgressionOrder = progressionOrder,
                };

                World<TWorld>.SetResource(configuration);
            }

            if (!World<TWorld>.HasResource<AbilityRegistry<TWorld>>())
            {
                var registry = new AbilityRegistry<TWorld>();
                World<TWorld>.SetResource(registry);
            }

            if (!World<TWorld>.HasResource<AbilityEffectDispatchRegistry<TWorld>>())
            {
                var dispatchRegistry =
                    new AbilityEffectDispatchRegistry<TWorld>();
                World<TWorld>.SetResource(dispatchRegistry);
            }

            if (!World<TWorld>.HasResource<IAbilityRng<TWorld>>())
            {
                IAbilityRng<TWorld> rng = new UnityAbilityRng<TWorld>();
                World<TWorld>.SetResource(rng);
            }

            if (!World<TWorld>.HasResource<AbilityStepActivatorRegistry<TWorld>>())
            {
                var activators = CreateActivators();
                World<TWorld>.SetResource(activators);
            }

            ref var config = ref World<TWorld>.GetResource<AbilityConfig>();
            if (!config.RegisterSystems)
                return UniTask.CompletedTask;
            World<TWorld>.Systems<StaticEcsUpdateSystems>.Add(
                new AbilityCastSystem<TWorld>(),
                config.CastOrder);
            World<TWorld>.Systems<StaticEcsUpdateSystems>.Add(
                new AbilityWaitSystem<TWorld>(),
                config.WaitOrder);
            World<TWorld>.Systems<StaticEcsUpdateSystems>.Add(
                new AbilityStepProgressionSystem<TWorld>(),
                config.ProgressionOrder);
            return UniTask.CompletedTask;
        }

        private static AbilityStepActivatorRegistry<TWorld> CreateActivators()
        {
            var activators = new AbilityStepActivatorRegistry<TWorld>();
            activators.Register<WaitStepConfig>(new WaitStepActivator<TWorld>());
            activators.Register<ApplyDamageStepConfig>(new ApplyDamageStepActivator<TWorld>());
            activators.Register<ApplyEffectStepConfig>(new ApplyEffectStepActivator<TWorld>());
            activators.Register<AoeQueryStepConfig>(new AoeQueryStepActivator<TWorld>());
            activators.Register<SetPrimaryTargetFromAoeStepConfig>(
                new SetPrimaryTargetFromAoeStepActivator<TWorld>());
            return activators;
        }
    }

    /// <summary>Controls ability system composition and execution order.</summary>
    public sealed class AbilityConfig : IResource
    {
        /// <summary>Whether the standard ability systems are installed.</summary>
        public bool RegisterSystems = true;

        /// <summary>Execution order of cast request processing.</summary>
        public short CastOrder = 150;

        /// <summary>Execution order of wait processing.</summary>
        public short WaitOrder = 155;

        /// <summary>Execution order of step progression.</summary>
        public short ProgressionOrder = 160;
    }
}
