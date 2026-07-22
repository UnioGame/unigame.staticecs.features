namespace UniGame.StaticEcs.Features
{
    using FFS.Libraries.StaticEcs;

    /// <summary>
    /// Bridges <see cref="StunEffect"/> lifecycle to the existing <c>StunOperations</c> source
    /// counter. Stack-up does not register additional sources — the counter is keyed by source
    /// entity, not by stack count.
    /// </summary>
    public sealed class StunEffectHandler<TWorld> : IEffectHandler<TWorld, StunEffect>
        where TWorld : struct, IWorldType
    {
        public void OnApplied(EntityGID target, EntityGID source, int stacks, int previousStacks)
        {
            if (previousStacks > 0)
            {
                return;
            }

            StunOperations.AddSource<TWorld>(target, source);
        }

        public void OnTick(EntityGID target, EntityGID source, int stacks) { }

        public void OnRemoved(EntityGID target, EntityGID source, int stacks, bool expired)
        {
            StunOperations.RemoveSource<TWorld>(target, source);
        }
    }
}
