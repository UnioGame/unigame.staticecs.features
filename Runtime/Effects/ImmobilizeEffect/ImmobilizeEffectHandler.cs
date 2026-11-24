namespace UniGame.StaticEcs.Features
{
    using FFS.Libraries.StaticEcs;

    /// <summary>
    /// Bridges <see cref="ImmobilizeEffect"/> lifecycle to the <c>ImmobilizeOperations</c> source counter.
    /// The effect contributes one entry keyed by the target itself, not by the applier: a reapply from another
    /// source overwrites <see cref="EffectComponent{TEffect}.Source"/>, so keying by the applier would leave the
    /// first entry behind on expiry. Manual sources keep their own entries.
    /// </summary>
    public class ImmobilizeEffectHandler<TWorld> : IEffectHandler<TWorld, ImmobilizeEffect>
        where TWorld : struct, IWorldType
    {
        public bool CanApply(in EffectApplyContext context) => true;

        public void OnApplied(in EffectApplyContext context)
        {
            if (context.PreviousStacks > 0)
                return;

            ImmobilizeOperations.AddSource<TWorld>(context.Target, context.Target);
        }

        public void OnTick(EntityGID target, EntityGID source, int stacks) { }

        public void OnRemoved(EntityGID target, EntityGID source, int stacks, bool expired)
        {
            ImmobilizeOperations.RemoveSource<TWorld>(target, target);
        }
    }
}
