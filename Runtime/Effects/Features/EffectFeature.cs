namespace UniGame.StaticEcs.Features
{
    using Cysharp.Threading.Tasks;
    using FFS.Libraries.StaticEcs;
    using UniGame.Core.Runtime;

    /// <summary>
    /// Generic effect feature: registers <see cref="EffectComponent{TEffect}"/>, lifecycle
    /// events, the shared <see cref="EffectSummaryComponent"/> roster, the per-effect handler, the
    /// stacking config, and <see cref="EffectTickSystem{TWorld,TEffect}"/> on the update group.
    ///
    /// Also wires the source-cleanup back-ref pipeline:
    /// <see cref="EffectTargetComponent"/>, <see cref="EffectTrackerComponent"/>, and a slot in
    /// <see cref="EffectRegistry"/> keyed by <see cref="EffectFlagOf{T}.Value"/>.
    ///
    /// Concrete effects derive from this feature and provide a <see cref="IEffectHandler{TWorld,TEffect}"/>
    /// implementation during initialization (for example <c>HealOverTimeFeature</c>).
    /// </summary>
    public class EffectFeature<TWorld, TEffect> : StaticEcsFeature<TWorld>
        where TWorld : struct, IWorldType
        where TEffect : struct, IEffectType
    {
        /// <summary>Default update order for typed effect tick systems.</summary>
        public const short DefaultTickOrder = 200;

        /// <summary>Gets the safe default stack limit installed by a concrete effect.</summary>
        protected virtual int DefaultMaxStacks => 1;

        /// <summary>Gets whether reapplication refreshes effect timing by default.</summary>
        protected virtual bool DefaultRefreshOnReapply => true;

        /// <summary>Creates the default handler supplied by a concrete effect feature.</summary>
        protected virtual IEffectHandler<TWorld, TEffect> CreateDefaultHandler() => null;

        /// <inheritdoc />
        public override UniTask InitializeAsync(ILifeTime lifeTime)
        {
            if (!World<TWorld>.HasResource<IEffectHandler<TWorld, TEffect>>())
            {
                var handler = CreateDefaultHandler();
                if (handler == null)
                    throw new System.InvalidOperationException(
                        $"Effect `{typeof(TEffect).FullName}` requires an " +
                        $"`IEffectHandler<{typeof(TWorld).Name}, {typeof(TEffect).Name}>` resource.");

                World<TWorld>.SetResource<IEffectHandler<TWorld, TEffect>>(handler);
            }

            if (!World<TWorld>.HasResource<EffectConfig<TWorld, TEffect>>())
            {
                var defaultConfig = new EffectConfig<TWorld, TEffect>(
                    DefaultMaxStacks,
                    DefaultRefreshOnReapply,
                    DefaultTickOrder,
                    true);

                World<TWorld>.SetResource(defaultConfig);
            }

            if (!World<TWorld>.HasResource<EffectIdRegistry>() ||
                !World<TWorld>.HasResource<EffectRegistry>())
                throw new System.InvalidOperationException(
                    $"Effect `{typeof(TEffect).FullName}` requires EffectsCoreFeature " +
                    "to install its registries first.");

            ref var idRegistry = ref World<TWorld>.GetResource<EffectIdRegistry>();
            idRegistry.Register<TEffect>();
            ref var effectRegistry = ref World<TWorld>.GetResource<EffectRegistry>();
            var flag = EffectFlagOf<TEffect>.Value;
            if (!effectRegistry.IsRegistered(flag))
                effectRegistry.Register(flag, RemoveOnSourceCleanup, RemoveUnconditional);

            ref var config = ref World<TWorld>.GetResource<EffectConfig<TWorld, TEffect>>();
            if (!config.RegisterTickSystem)
                return UniTask.CompletedTask;

            World<TWorld>.Systems<StaticEcsUpdateSystems>.Add(
                new EffectTickSystem<TWorld, TEffect>(),
                config.TickOrder);
            return UniTask.CompletedTask;
        }

        private static void RemoveOnSourceCleanup(EntityGID source, EntityGID target)
        {
            EffectOperations.RemoveFromSource<TWorld, TEffect>(target, source);
        }

        private static void RemoveUnconditional(EntityGID target)
        {
            EffectOperations.Remove<TWorld, TEffect>(target);
        }
    }
}
