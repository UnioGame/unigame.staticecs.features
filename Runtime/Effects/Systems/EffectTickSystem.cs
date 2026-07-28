namespace UniGame.StaticEcs.Features
{
    using FFS.Libraries.StaticEcs;
    using Time;

    /// <summary>
    /// Drains <see cref="EffectComponent{TEffect}"/> timers, fires periodic
    /// <see cref="IEffectHandler{TWorld,TEffect}.OnTick"/> calls, and removes the effect on
    /// natural expiry. Source-destroy cleanup is push-driven by
    /// <see cref="EffectTrackerComponent"/> and the <see cref="EffectRegistry"/> back-ref pipeline,
    /// so this system does not poll <c>Source.Status</c>.
    /// </summary>
    public class EffectTickSystem<TWorld, TEffect> : ISystem
        where TWorld : struct, IWorldType
        where TEffect : struct, IEffectType
    {
        public void Update()
        {
            var dt = World<TWorld>.GetResource<EcsTime>().DeltaTime;
            if (dt <= 0f)
                return;

            ref var handler = ref World<TWorld>.GetResource<IEffectHandler<TWorld, TEffect>>();

            foreach (var entity in World<TWorld>.Query<All<EffectComponent<TEffect>>>().Entities())
            {
                ref var data = ref entity.Mut<EffectComponent<TEffect>>();
                data.TimeLeft -= dt;

                if (data.Period > 0f)
                {
                    data.PeriodLeft -= dt;
                    while (data.PeriodLeft <= 0f && data.TimeLeft > 0f)
                    {
                        handler.OnTick(entity.GID, data.Source, data.Stacks);
                        data.PeriodLeft += data.Period;
                    }
                }

                if (data.TimeLeft <= 0f)
                    EffectOperations.Expire<TWorld, TEffect>(entity, entity.GID);
            }

            foreach (var entity in
                     World<TWorld>.Query<All<PendingEffectComponent<TEffect>>>().Entities())
            {
                ref var pending = ref entity.Mut<PendingEffectComponent<TEffect>>();
                pending.DelayLeft -= dt;
                if (pending.DelayLeft <= 0f)
                    EffectOperations.ActivatePending<TWorld, TEffect>(
                        entity,
                        entity.GID);
            }
        }
    }
}
