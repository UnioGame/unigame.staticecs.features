namespace UniGame.StaticEcs.Features
{
    using FFS.Libraries.StaticEcs;

    /// <summary>
    /// Routes <see cref="HealOverTimeEffect"/> ticks through <c>DamageOperations.RaiseHealing</c>
    /// using the per-target <see cref="HealOverTimeComponent"/> payload, scaled by current stacks.
    /// </summary>
    public sealed class HealOverTimeHandler<TWorld> : IEffectHandler<TWorld, HealOverTimeEffect>
        where TWorld : struct, IWorldType
    {
        public void OnApplied(
            EntityGID target,
            EntityGID source,
            int stacks,
            int previousStacks
        ) { }

        public void OnTick(EntityGID target, EntityGID source, int stacks)
        {
            if (!target.TryUnpack<TWorld>(out var entity))
            {
                return;
            }

            if (!entity.Has<HealOverTimeComponent>())
            {
                return;
            }

            var amount = entity.Read<HealOverTimeComponent>().HealPerTick;
            if (amount <= 0f)
            {
                return;
            }

            DamageOperations.RaiseHealing<TWorld>(source, target, amount * stacks);
        }

        public void OnRemoved(EntityGID target, EntityGID source, int stacks, bool expired) { }
    }
}
