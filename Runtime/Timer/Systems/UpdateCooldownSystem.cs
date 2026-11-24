namespace UniGame.StaticEcs.Features
{
    using System.Collections.Generic;
    using FFS.Libraries.StaticEcs;
    using UniGame.StaticEcs.Time;

    /// <summary>Updates active timers and reports each completed run once.</summary>
    public class UpdateCooldownSystem<TWorld> : ISystem
        where TWorld : struct, IWorldType
    {
        private readonly List<EntityGID> _finished = new();

        public void Update()
        {
            var deltaTime = World<TWorld>.GetResource<EcsTime>().DeltaTime;
            _finished.Clear();

            foreach (var entity in World<TWorld>
                         .Query<All<CooldownComponent, CooldownRemainingComponent, CooldownActiveTag>>()
                         .Entities())
            {
                ref var cooldown = ref entity.Mut<CooldownRemainingComponent>();
                var step = TimerMillisecondOperations.TakeStep(deltaTime,
                    ref cooldown.FractionalMilliseconds);
                cooldown.RemainingMilliseconds = step >= cooldown.RemainingMilliseconds
                    ? 0u
                    : cooldown.RemainingMilliseconds - step;
                if (cooldown.RemainingMilliseconds == 0u)
                    _finished.Add(entity.GID);
            }

            foreach (var target in _finished)
            {
                if (!target.TryUnpack<TWorld>(out var entity) ||
                    !entity.Has<CooldownComponent>() ||
                    !entity.Has<CooldownRemainingComponent>() ||
                    !entity.Has<CooldownActiveTag>())
                    continue;

                entity.Delete<CooldownActiveTag>();
                entity.Set<CooldownCompleteTag>();
                World<TWorld>.SendEvent(new CooldownFinishedEvent { Entity = target });
            }
        }
    }
}
