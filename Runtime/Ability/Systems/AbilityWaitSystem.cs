namespace UniGame.StaticEcs.Features
{
    using FFS.Libraries.StaticEcs;
    using Time;

    /// <summary>
    /// Drains <see cref="AbilityWaitComponent"/> timers across all active cast-entities. On expiry
    /// removes the wait component, writes <see cref="StepStatus.Success"/> into
    /// <see cref="AbilityStepStatusComponent"/>, and arms <see cref="AbilityStepReadyTag"/> so
    /// <c>AbilityStepProgressionSystem</c> can advance the cast in the next pass.
    /// </summary>
    public class AbilityWaitSystem<TWorld> : ISystem
        where TWorld : struct, IWorldType
    {
        public void Update()
        {
            var dt = World<TWorld>.GetResource<EcsTime>().DeltaTime;
            if (dt <= 0f)
            {
                return;
            }

            foreach (var entity in World<TWorld>.Query<All<AbilityWaitComponent>>().Entities())
            {
                ref var state = ref entity.Mut<AbilityWaitComponent>();
                state.TimeLeft -= dt;
                if (state.TimeLeft > 0f)
                {
                    continue;
                }

                entity.Delete<AbilityWaitComponent>();
                entity.Set(new AbilityStepStatusComponent { Status = StepStatus.Success });
                entity.Set<AbilityStepReadyTag>();
            }
        }
    }
}
