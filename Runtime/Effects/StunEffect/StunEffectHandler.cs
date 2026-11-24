namespace UniGame.StaticEcs.Features
{
    using FFS.Libraries.StaticEcs;

    /// <summary>
    /// Bridges <see cref="StunEffect"/> lifecycle to the existing <c>StunOperations</c> source
    /// counter. Stack-up does not register additional sources — the counter is keyed by source
    /// entity, not by stack count.
    /// </summary>
    public class StunEffectHandler<TWorld> : IEffectHandler<TWorld, StunEffect>
        where TWorld : struct, IWorldType
    {
        public bool CanApply(in EffectApplyContext context) => true;

        public void OnApplied(in EffectApplyContext context)
        {
            if (context.PreviousStacks > 0)
                return;

            StunOperations.AddSource<TWorld>(context.Target, context.Source);
        }

        public void OnTick(EntityGID target, EntityGID source, int stacks) { }

        public void OnRemoved(EntityGID target, EntityGID source, int stacks, bool expired)
        {
            StunOperations.RemoveSource<TWorld>(target, source);
        }
    }
}
