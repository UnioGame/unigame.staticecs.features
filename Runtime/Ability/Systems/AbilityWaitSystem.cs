using FFS.Libraries.StaticEcs;
 

namespace UniGame.StaticEcs.Features {
    using Time;

    /// <summary>
    /// Drains <see cref="AbilityWaitState"/> timers across all active cast-entities. On expiry
    /// removes the wait component, writes <see cref="StepStatus.Success"/> into
    /// <see cref="AbilityStepLastStatus"/>, and arms <see cref="AbilityStepReadyTag"/> so
    /// <c>AbilityStepProgressionSystem</c> can advance the cast in the next pass.
    /// </summary>
    public sealed class AbilityWaitSystem<TWorld> : ISystem
        where TWorld : struct, IWorldType {
        public void Update() {
            var dt = World<TWorld>.GetResource<EcsTime>().DeltaTime;
            if (dt <= 0f) {
                return;
            }

            foreach (var entity in World<TWorld>
                         .Query<All<AbilityWaitState>>()
                         .Entities()) {
                ref var state = ref entity.Mut<AbilityWaitState>();
                state.TimeLeft -= dt;
                if (state.TimeLeft > 0f) {
                    continue;
                }

                entity.Delete<AbilityWaitState>();
                entity.Set(new AbilityStepLastStatus { Status = StepStatus.Success });
                entity.Set<AbilityStepReadyTag>();
            }
        }
    }
}
