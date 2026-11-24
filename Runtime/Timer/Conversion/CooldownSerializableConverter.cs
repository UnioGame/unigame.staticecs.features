namespace UniGame.StaticEcs.Features
{
    using System;
    using FFS.Libraries.StaticEcs;
    using UniGame.StaticEcs.Unity;
    using UnityEngine;

    /// <summary>Starts an entity cooldown from inline authoring settings.</summary>
    [Serializable]
    public class CooldownSerializableConverter<TWorld> : EcsSerializableConverter<TWorld>
        where TWorld : struct, IWorldType
    {
        public float duration = 1f;
        public bool autoRestart;
        public bool destroyOnComplete;

        public override void Apply(World<TWorld>.Entity entity, GameObject host)
        {
            entity.Set(new CooldownComponent
            {
                DurationMilliseconds = TimerMillisecondOperations.FromSeconds(duration)
            });

            if (autoRestart)
                entity.Set<CooldownAutoRestartTag>();
            else
                entity.Delete<CooldownAutoRestartTag>();

            if (destroyOnComplete)
                entity.Set<CooldownDestroyOnCompleteTag>();
            else
                entity.Delete<CooldownDestroyOnCompleteTag>();

            CooldownOperations.Restart<TWorld>(entity.GID);
        }
    }
}
