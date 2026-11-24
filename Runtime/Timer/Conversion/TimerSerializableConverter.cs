namespace UniGame.StaticEcs.Features
{
    using System;
    using FFS.Libraries.StaticEcs;
    using UniGame.StaticEcs.Unity;
    using UnityEngine;

    /// <summary>Configures and starts an entity-owned count-up timer.</summary>
    [Serializable]
    public class TimerSerializableConverter<TWorld> : EcsSerializableConverter<TWorld>
        where TWorld : struct, IWorldType
    {
        /// <summary>Zero disables the completion limit.</summary>
        public float limitSeconds;

        public override void Apply(World<TWorld>.Entity entity, GameObject host)
        {
            entity.Set(new TimerComponent
            {
                LimitMilliseconds = TimerMillisecondOperations.FromSeconds(limitSeconds)
            });
            TimerOperations.Reset<TWorld>(entity.GID);
            TimerOperations.Start<TWorld>(entity.GID);
        }
    }
}
