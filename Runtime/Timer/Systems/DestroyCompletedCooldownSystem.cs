namespace UniGame.StaticEcs.Features
{
    using System.Collections.Generic;
    using FFS.Libraries.StaticEcs;

    /// <summary>Destroys entities marked for deletion after their timer finishes.</summary>
    public class DestroyCompletedCooldownSystem<TWorld> : ISystem
        where TWorld : struct, IWorldType
    {
        private readonly List<EntityGID> _pending = new();

        public void Update()
        {
            _pending.Clear();
            foreach (var entity in World<TWorld>
                         .Query<All<CooldownComponent, CooldownCompleteTag,
                             CooldownDestroyOnCompleteTag>>()
                         .Entities())
                _pending.Add(entity.GID);

            foreach (var target in _pending)
            {
                if (target.TryUnpack<TWorld>(out var entity))
                    entity.Destroy();
            }
        }
    }
}
