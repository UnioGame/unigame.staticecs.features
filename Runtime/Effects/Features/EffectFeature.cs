using System.Threading;
using Cysharp.Threading.Tasks;
using FFS.Libraries.StaticEcs;

namespace UniGame.StaticEcs.Features {
    /// <summary>
    /// Generic effect feature: registers <see cref="EffectComponent{TEffect}"/>, lifecycle
    /// events, the shared <see cref="EffectRosterEntry"/> roster, the per-effect handler, the
    /// stacking config, and <see cref="EffectTickSystem{TWorld,TEffect}"/> on the update group.
    ///
    /// Also wires the source-cleanup back-ref pipeline:
    /// <see cref="EffectBackRef"/>, <see cref="EffectSourceTracker"/>, and a slot in
    /// <see cref="EffectRegistry"/> keyed by <see cref="EffectFlagOf{T}.Value"/>.
    ///
    /// Concrete effects derive from this feature and provide a <see cref="IEffectHandler{TWorld,TEffect}"/>
    /// implementation in their own RegisterTypes call (e.g. <c>HealOverTimeFeature</c>).
    /// </summary>
    public class EffectFeature<TWorld, TEffect> :
        StaticEcsFeature<TWorld>,
        IStaticEcsSystemsFeature<TWorld, StaticEcsUpdateSystems>
        where TWorld : struct, IWorldType
        where TEffect : struct, IEffectType {
        public const short DefaultTickOrder = 200;

        private readonly IEffectHandler<TWorld, TEffect> _handler;
        private readonly int _maxStacks;
        private readonly bool _refreshOnReapply;
        private readonly short _tickOrder;
        private readonly bool _registerTickSystem;

        public EffectFeature(
            IEffectHandler<TWorld, TEffect> handler,
            int maxStacks = 1,
            bool refreshOnReapply = true,
            short tickOrder = DefaultTickOrder,
            bool registerTickSystem = true) {
            _handler = handler;
            _maxStacks = maxStacks;
            _refreshOnReapply = refreshOnReapply;
            _tickOrder = tickOrder;
            _registerTickSystem = registerTickSystem;
        }

        public override void RegisterTypes(World<TWorld>.TypeRegistrar types) {
            // Resolve flag eagerly — throws InvalidOperationException if [EffectFlag] is missing.
            var flag = EffectFlagOf<TEffect>.Value;

            // Per-effect types: unique per closed TEffect, always safe to register.
            types
                .Component<EffectComponent<TEffect>>()
                .Event<EffectAppliedEvent<TEffect>>()
                .Event<EffectRefreshedEvent<TEffect>>()
                .Event<EffectRemovedEvent<TEffect>>();

            // Shared types: registered once per world. EffectIdRegistry resource doubles as
            // the "shared types already installed" sentinel — its presence is set up by the
            // first EffectFeature that runs, so subsequent features skip the duplicate register.
            if (!World<TWorld>.HasResource<EffectIdRegistry>()) {
                types
                    .Component<EffectSourceTracker>()
                    .Multi<EffectRosterEntry>()
                    .Multi<EffectBackRef>();

                World<TWorld>.SetResource(new EffectIdRegistry());
            }

            ref var idRegistry = ref World<TWorld>.GetResource<EffectIdRegistry>();
            idRegistry.Register<TEffect>();

            if (!World<TWorld>.HasResource<EffectRegistry>()) {
                World<TWorld>.SetResource(new EffectRegistry());
            }

            ref var effectRegistry = ref World<TWorld>.GetResource<EffectRegistry>();
            if (!effectRegistry.IsRegistered(flag)) {
                effectRegistry.Register(flag, RemoveOnSourceCleanup, RemoveUnconditional);
            }

            if (!World<TWorld>.HasResource<IEffectHandler<TWorld, TEffect>>()) {
                World<TWorld>.SetResource<IEffectHandler<TWorld, TEffect>>(_handler);
            }

            if (!World<TWorld>.HasResource<EffectConfig<TWorld, TEffect>>()) {
                World<TWorld>.SetResource(new EffectConfig<TWorld, TEffect>(_maxStacks, _refreshOnReapply));
            }
        }

        public UniTask RegisterSystemsAsync(
            StaticEcsSystemsBuilder<TWorld, StaticEcsUpdateSystems> systems,
            CancellationToken cancellationToken) {
            if (!_registerTickSystem) {
                return UniTask.CompletedTask;
            }

            systems.Add(new EffectTickSystem<TWorld, TEffect>(), _tickOrder);
            return UniTask.CompletedTask;
        }

        private static void RemoveOnSourceCleanup(EntityGID source, EntityGID target) {
            EffectOperations.RemoveFromSource<TWorld, TEffect>(target, source);
        }

        private static void RemoveUnconditional(EntityGID target) {
            EffectOperations.Remove<TWorld, TEffect>(target);
        }
    }
}
