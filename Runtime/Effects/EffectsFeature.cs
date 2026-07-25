namespace UniGame.StaticEcs.Features
{
    using Cysharp.Threading.Tasks;
    using FFS.Libraries.StaticEcs;
    using UniGame.Core.Runtime;
    using UniGame.StaticEcs.Time;

    /// <summary>Composes the standard heal-over-time, stun, and speed-modification effects.</summary>
    public class EffectsFeature<TWorld> : StaticEcsFeature<TWorld>
        where TWorld : struct, IWorldType
    {
        /// <summary>Maximum number of simultaneous heal-over-time stacks.</summary>
        public int healOverTimeMaxStacks = 5;

        /// <summary>Whether applying an existing effect refreshes its duration.</summary>
        public bool refreshOnReapply = true;

        /// <summary>Execution order of effect tick systems.</summary>
        public short tickOrder = EffectFeature<TWorld, StunEffect>.DefaultTickOrder;

        /// <summary>Whether standard effect tick systems are installed.</summary>
        public bool registerTickSystems = true;

        /// <inheritdoc />
        public override async UniTask InitializeAsync(ILifeTime lifeTime)
        {
            if (!World<TWorld>.HasResource<EffectConfig<TWorld, HealOverTimeEffect>>())
            {
                var healOverTimeConfig =
                    new EffectConfig<TWorld, HealOverTimeEffect>(
                        healOverTimeMaxStacks,
                        refreshOnReapply,
                        tickOrder,
                        registerTickSystems);

                World<TWorld>.SetResource(healOverTimeConfig);
            }

            if (!World<TWorld>.HasResource<EffectConfig<TWorld, StunEffect>>())
            {
                var stunConfig = new EffectConfig<TWorld, StunEffect>(
                    1,
                    refreshOnReapply,
                    tickOrder,
                    registerTickSystems);

                World<TWorld>.SetResource(stunConfig);
            }

            if (!World<TWorld>.HasResource<
                    EffectConfig<TWorld, ModificationEffect<SpeedCharacteristic>>>())
            {
                var modificationConfig =
                    new EffectConfig<TWorld, ModificationEffect<SpeedCharacteristic>>(
                        1,
                        refreshOnReapply,
                        tickOrder,
                        registerTickSystems);

                World<TWorld>.SetResource(modificationConfig);
            }

            await new EffectsCoreFeature<TWorld>().InitializeAsync(lifeTime);
            await new HealOverTimeFeature<TWorld>().InitializeAsync(lifeTime);
            await new StunEffectFeature<TWorld>().InitializeAsync(lifeTime);
            await new ModificationEffectFeature<TWorld, SpeedCharacteristic>()
                .InitializeAsync(lifeTime);
        }
    }
}
