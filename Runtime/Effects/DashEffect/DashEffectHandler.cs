namespace UniGame.StaticEcs.Features
{
    using FFS.Libraries.StaticEcs;

    /// <summary>
    /// Starts a dash from the direction of <see cref="MovementEffectComponent"/>, the distance given as the
    /// effect magnitude and the lifetime of the effect. A dash that is still running is not restarted or extended.
    /// </summary>
    public class DashEffectHandler<TWorld> : IEffectHandler<TWorld, DashEffect>
        where TWorld : struct, IWorldType
    {
        public bool CanApply(in EffectApplyContext context)
        {
            return context.PreviousStacks == 0 &&
                   context.Magnitude > 0f &&
                   float.IsFinite(context.Magnitude) &&
                   MovementEffectOperations.Has<TWorld>(context.Target);
        }

        public void OnApplied(in EffectApplyContext context)
        {
            if (context.PreviousStacks > 0 ||
                !context.Target.TryUnpack<TWorld>(out var entity) ||
                !entity.Has<MovementEffectComponent>() ||
                !entity.Has<EffectComponent<DashEffect>>())
                return;

            var aim = entity.Read<MovementEffectComponent>();
            entity.Delete<MovementEffectComponent>();
            entity.Set(new DashComponent
            {
                DirectionX = aim.DirectionX,
                DirectionZ = aim.DirectionZ,
                Distance = context.Magnitude,
                Duration = entity.Read<EffectComponent<DashEffect>>().TimeLeft,
            });
        }

        public void OnTick(EntityGID target, EntityGID source, int stacks) { }

        public void OnRemoved(EntityGID target, EntityGID source, int stacks, bool expired)
        {
            DashOperations.Cancel<TWorld>(target);
        }
    }
}
