namespace UniGame.StaticEcs.Features
{
    using Cysharp.Threading.Tasks;
    using FFS.Libraries.StaticEcs;
    using UniGame.Core.Runtime;

    /// <summary>
    /// Wires the damage pipeline: registers events, tags, the default filter chain
    /// (Dodge → Block → ArmorResist → Critical → Shield), an <see cref="IDamageRng"/>
    /// resource, and the <see cref="ApplyDamageSystem{TWorld}"/> on the update group.
    ///
    /// Dependencies that must be registered alongside this feature:
    /// <see cref="HealthFeature{TWorld}"/>, <see cref="ShieldFeature{TWorld}"/>, and
    /// <see cref="CharacteristicFeature{TWorld, TCharacteristic}"/> for each combat stat.
    /// </summary>
    public class DamageFeature<TWorld> : StaticEcsFeature<TWorld>
        where TWorld : struct, IWorldType
    {
        public const short DefaultApplyOrder = 100;

        /// <summary>Whether the damage application system is installed.</summary>
        public bool registerApplySystem = true;

        /// <summary>Whether the standard filter chain is installed.</summary>
        public bool registerDefaultChain = true;

        /// <summary>Execution order of damage application.</summary>
        public short applyOrder = DefaultApplyOrder;

        /// <inheritdoc />
        public override UniTask InitializeAsync(ILifeTime lifeTime)
        {
            if (!World<TWorld>.HasResource<DamageConfig>())
            {
                var configuration = new DamageConfig
                {
                    RegisterApplySystem = registerApplySystem,
                    RegisterDefaultChain = registerDefaultChain,
                    ApplyOrder = applyOrder,
                };

                World<TWorld>.SetResource(configuration);
            }

            if (!World<TWorld>.HasResource<IDamageRng>())
            {
                IDamageRng rng = new UnityDamageRng();
                World<TWorld>.SetResource(rng);
            }

            ref var config = ref World<TWorld>.GetResource<DamageConfig>();
            if (config.RegisterDefaultChain &&
                !World<TWorld>.HasResource<DamageFilterChain<TWorld>>())
            {
                var chain = new DamageFilterChain<TWorld>();
                chain.Add(new DodgeFilter<TWorld>());
                chain.Add(new BlockFilter<TWorld>());
                chain.Add(new ArmorResistFilter<TWorld>());
                chain.Add(new CriticalFilter<TWorld>());
                chain.Add(new ShieldFilter<TWorld>());
                World<TWorld>.SetResource(chain);
            }

            var updateEnabled =
                World<TWorld>.HasResource<Unity.StaticEcsSystemsConfig>() &&
                World<TWorld>.GetResource<Unity.StaticEcsSystemsConfig>().update;
            if (!updateEnabled || !config.RegisterApplySystem)
            {
                return UniTask.CompletedTask;
            }

            World<TWorld>.Systems<StaticEcsUpdateSystems>.Add(
                new ApplyDamageSystem<TWorld>(),
                config.ApplyOrder);
            return UniTask.CompletedTask;
        }
    }

    /// <summary>Controls damage pipeline composition and execution order.</summary>
    public sealed class DamageConfig : IResource
    {
        /// <summary>Whether the damage application system is installed.</summary>
        public bool RegisterApplySystem = true;

        /// <summary>Whether the standard filter chain is installed.</summary>
        public bool RegisterDefaultChain = true;

        /// <summary>Execution order of damage application.</summary>
        public short ApplyOrder = 100;
    }
}
