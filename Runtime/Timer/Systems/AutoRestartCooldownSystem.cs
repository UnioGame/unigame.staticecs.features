namespace UniGame.StaticEcs.Features
{
    using System.Collections.Generic;
    using FFS.Libraries.StaticEcs;

    /// <summary>Restarts recurring timers one update after completion.</summary>
    public class AutoRestartCooldownSystem<TWorld> : ISystem
        where TWorld : struct, IWorldType
    {
        private readonly List<EntityGID> _pending = new();

        public void Update()
        {
            foreach (var target in _pending)
            {
                if (target.TryUnpack<TWorld>(out var entity) &&
                    entity.Has<CooldownComponent>() &&
                    entity.Has<CooldownCompleteTag>() &&
                    entity.Has<CooldownAutoRestartTag>() &&
                    !entity.Has<CooldownDestroyOnCompleteTag>())
                    CooldownOperations.Restart<TWorld>(target);
            }

            _pending.Clear();
            foreach (var entity in World<TWorld>
                         .Query<All<CooldownComponent, CooldownCompleteTag, CooldownAutoRestartTag>,
                             None<CooldownDestroyOnCompleteTag>>()
                         .Entities())
                _pending.Add(entity.GID);
        }
    }
}
