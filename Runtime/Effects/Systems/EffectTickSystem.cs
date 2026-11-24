namespace UniGame.StaticEcs.Features
{
    using System.Collections.Generic;
    using FFS.Libraries.StaticEcs;
    using Time;

    /// <summary>
    /// Drains <see cref="EffectComponent{TEffect}"/> timers, fires activations
    /// (<see cref="IEffectHandler{TWorld,TEffect}.OnTick"/>) and removes the effect when its lifetime ends.
    /// The configured <see cref="EffectDurationMode"/> decides what ends the effect, the
    /// <see cref="EffectActivationMode"/> decides when it activates. Source-destroy cleanup is push-driven by
    /// <see cref="EffectTrackerComponent"/> and the <see cref="EffectRegistry"/> back-ref pipeline,
    /// so this system does not poll <c>Source.Status</c>.
    /// </summary>
    /// <remarks>
    /// An activation that falls on the exact expiry moment still fires, so a 3 second effect with a 1 second
    /// period activates exactly three times. The epsilon absorbs float drift of the accumulated deltas.
    /// </remarks>
    public class EffectTickSystem<TWorld, TEffect> : ISystem
        where TWorld : struct, IWorldType
        where TEffect : struct, IEffectType
    {
        private const float TimeEpsilon = EffectLifetime.TimeEpsilon;

        private readonly List<EntityGID> _expired = new();
        private readonly List<EntityGID> _activated = new();

        public void Update()
        {
            var dt = World<TWorld>.GetResource<EcsTime>().DeltaTime;
            if (dt <= 0f)
                return;

            ref var handler = ref World<TWorld>.GetResource<IEffectHandler<TWorld, TEffect>>();
            var config = World<TWorld>.GetResource<EffectConfig<TWorld, TEffect>>();
            var spendsCharges = config.DurationMode == EffectDurationMode.Charges;
            var activatesOnCondition = config.ActivationMode == EffectActivationMode.Condition;
            var hasCondition = World<TWorld>.HasResource<IEffectCondition<TWorld, TEffect>>();
            _expired.Clear();
            _activated.Clear();

            foreach (var entity in World<TWorld>.Query<All<EffectComponent<TEffect>>>().Entities())
            {
                ref var data = ref entity.Mut<EffectComponent<TEffect>>();
                if (!spendsCharges)
                    data.TimeLeft -= dt;

                if (activatesOnCondition)
                {
                    if (hasCondition &&
                        (!spendsCharges || data.ChargesLeft > 0) &&
                        World<TWorld>.GetResource<IEffectCondition<TWorld, TEffect>>()
                            .IsMet(entity.GID, data.Source))
                    {
                        handler.OnTick(entity.GID, data.Source, data.Stacks);
                        if (spendsCharges)
                            data.ChargesLeft--;
                    }
                }
                else if (data.Period > 0f)
                {
                    data.PeriodLeft -= dt;
                    while (data.PeriodLeft <= TimeEpsilon &&
                           (spendsCharges ? data.ChargesLeft > 0 : data.TimeLeft > -TimeEpsilon))
                    {
                        handler.OnTick(entity.GID, data.Source, data.Stacks);
                        data.PeriodLeft += data.Period;
                        if (spendsCharges)
                            data.ChargesLeft--;
                    }
                }

                if (EffectLifetime.IsExhausted(config.DurationMode, data.TimeLeft, data.ChargesLeft))
                    _expired.Add(entity.GID);
            }

            foreach (var entity in
                     World<TWorld>.Query<All<PendingEffectComponent<TEffect>>>().Entities())
            {
                ref var pending = ref entity.Mut<PendingEffectComponent<TEffect>>();
                pending.DelayLeft -= dt;
                if (pending.DelayLeft <= 0f)
                    _activated.Add(entity.GID);
            }

            // Expiry and activation run handlers that may remove other effects, so they follow the query pass.
            foreach (var gid in _expired)
            {
                if (gid.TryUnpack<TWorld>(out var expiredEntity) &&
                    expiredEntity.Has<EffectComponent<TEffect>>())
                    EffectOperations.Expire<TWorld, TEffect>(expiredEntity, gid);
            }

            foreach (var gid in _activated)
            {
                if (gid.TryUnpack<TWorld>(out var activatedEntity))
                    EffectOperations.ActivatePending<TWorld, TEffect>(activatedEntity, gid);
            }
        }
    }
}
