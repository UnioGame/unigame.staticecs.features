namespace UniGame.StaticEcs.Features
{
    using System.Collections.Generic;
    using FFS.Libraries.StaticEcs;
    using UniGame.StaticEcs.Time;

    /// <summary>Advances active count-up timers using scaled ECS time.</summary>
    public class UpdateTimerSystem<TWorld> : ISystem
        where TWorld : struct, IWorldType
    {
        private readonly List<EntityGID> _active = new();

        public void Update()
        {
            var deltaTime = World<TWorld>.GetResource<EcsTime>().DeltaTime;
            _active.Clear();
            foreach (var entity in World<TWorld>
                         .Query<All<TimerComponent, TimerActiveTag>>()
                         .Entities())
                _active.Add(entity.GID);

            foreach (var target in _active)
            {
                if (!target.TryUnpack<TWorld>(out var entity) ||
                    !entity.Has<TimerComponent>() ||
                    !entity.Has<TimerActiveTag>())
                    continue;

                ref var timer = ref entity.Mut<TimerComponent>();
                var step = TimerMillisecondOperations.TakeStep(deltaTime,
                    ref timer.FractionalMilliseconds);
                var elapsed = (ulong)timer.ElapsedMilliseconds + step;
                if (timer.LimitMilliseconds > 0u &&
                    elapsed >= timer.LimitMilliseconds)
                {
                    timer.ElapsedMilliseconds = timer.LimitMilliseconds;
                    timer.FractionalMilliseconds = 0f;
                }
                else
                {
                    timer.ElapsedMilliseconds = elapsed >= uint.MaxValue
                        ? uint.MaxValue
                        : (uint)elapsed;
                }

                if (timer.LimitMilliseconds == 0u ||
                    timer.ElapsedMilliseconds < timer.LimitMilliseconds)
                    continue;

                entity.Delete<TimerActiveTag>();
                entity.Set<TimerCompleteTag>();
                World<TWorld>.SendEvent(new TimerFinishedEvent { Entity = target });
            }
        }
    }
}
