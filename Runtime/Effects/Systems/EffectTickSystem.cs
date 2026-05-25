using FFS.Libraries.StaticEcs;
using unigame.staticecs.Time;

namespace unigame.staticecs.features {
    /// <summary>
    /// Drains <see cref="EffectComponent{TEffect}"/> timers, fires periodic
    /// <see cref="IEffectHandler{TWorld,TEffect}.OnTick"/> calls, and removes the effect on
    /// natural expiry. Source-destroy cleanup is push-driven by
    /// <see cref="EffectSourceTracker"/> and the <see cref="EffectRegistry"/> back-ref pipeline,
    /// so this system does not poll <c>Source.Status</c>.
    /// </summary>
    public sealed class EffectTickSystem<TWorld, TEffect> : ISystem
        where TWorld : struct, IWorldType
        where TEffect : struct, IEffectType {
        public void Update() {
            var dt = World<TWorld>.GetResource<EcsTime>().DeltaTime;
            if (dt <= 0f) {
                return;
            }

            ref var handler = ref World<TWorld>.GetResource<IEffectHandler<TWorld, TEffect>>();

            foreach (var entity in World<TWorld>
                         .Query<All<EffectComponent<TEffect>>>()
                         .Entities()) {
                ref var data = ref entity.Mut<EffectComponent<TEffect>>();

                if (data.DelayLeft > 0f) {
                    data.DelayLeft -= dt;
                    if (data.DelayLeft < 0f) {
                        data.DelayLeft = 0f;
                    }
                }

                data.TimeLeft -= dt;

                if (data.Period > 0f) {
                    data.PeriodLeft -= dt;
                    while (data.DelayLeft <= 0f && data.PeriodLeft <= 0f && data.TimeLeft > 0f) {
                        handler.OnTick(entity.GID, data.Source, data.Stacks);
                        data.PeriodLeft += data.Period;
                    }
                }

                if (data.TimeLeft <= 0f) {
                    EffectOperations.Expire<TWorld, TEffect>(entity, entity.GID);
                }
            }
        }
    }
}
