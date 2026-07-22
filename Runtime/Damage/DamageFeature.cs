namespace UniGame.StaticEcs.Features
{
    using System.Threading;
    using Cysharp.Threading.Tasks;
    using FFS.Libraries.StaticEcs;

    /// <summary>
    /// Wires the damage pipeline: registers events, tags, the default filter chain
    /// (Dodge → Block → ArmorResist → Critical → Shield), an <see cref="IDamageRng"/>
    /// resource, and the <see cref="ApplyDamageSystem{TWorld}"/> on the update group.
    ///
    /// Dependencies that must be registered alongside this feature:
    /// <see cref="HealthFeature{TWorld}"/>, <see cref="ShieldFeature{TWorld}"/>, and
    /// <see cref="CharacteristicFeature{TWorld, TCharacteristic}"/> for each combat stat.
    /// </summary>
    public class DamageFeature<TWorld>
        : StaticEcsFeature<TWorld>,
            IStaticEcsSystemsFeature<TWorld, StaticEcsUpdateSystems>
        where TWorld : struct, IWorldType
    {
        public const short DefaultApplyOrder = 100;

        /// <summary>Whether the damage application system is installed.</summary>
        public bool registerApplySystem = true;

        /// <summary>Whether the default damage filter chain is installed.</summary>
        public bool registerDefaultChain = true;

        /// <summary>Execution order of damage application.</summary>
        public short applyOrder = DefaultApplyOrder;

        public DamageFeature(
            bool registerApplySystem = true,
            bool registerDefaultChain = true,
            short applyOrder = DefaultApplyOrder
        )
        {
            this.registerApplySystem = registerApplySystem;
            this.registerDefaultChain = registerDefaultChain;
            this.applyOrder = applyOrder;
        }

        public override void RegisterTypes(World<TWorld>.TypeRegistrar types)
        {
            types
                .Tag<BlockableTag>()
                .Tag<DeathPendingTag>()
                .Event<IncomingDamageEvent>()
                .Event<DamageDodgedEvent>()
                .Event<DamageBlockedEvent>()
                .Event<DamageCriticalEvent>()
                .Event<ShieldDeltaEvent>()
                .Event<DamageAppliedEvent>();

            if (!World<TWorld>.HasResource<IDamageRng>())
            {
                World<TWorld>.SetResource<IDamageRng>(new UnityDamageRng());
            }

            if (registerDefaultChain && !World<TWorld>.HasResource<DamageFilterChain<TWorld>>())
            {
                var chain = new DamageFilterChain<TWorld>()
                    .Add(new DodgeFilter<TWorld>())
                    .Add(new BlockFilter<TWorld>())
                    .Add(new ArmorResistFilter<TWorld>())
                    .Add(new CriticalFilter<TWorld>())
                    .Add(new ShieldFilter<TWorld>());
                World<TWorld>.SetResource(chain);
            }
        }

        public UniTask RegisterSystemsAsync(
            StaticEcsSystemsBuilder<TWorld, StaticEcsUpdateSystems> systems,
            CancellationToken cancellationToken
        )
        {
            if (!registerApplySystem)
            {
                return UniTask.CompletedTask;
            }

            systems.Add(new ApplyDamageSystem<TWorld>(), applyOrder);
            return UniTask.CompletedTask;
        }
    }
}
