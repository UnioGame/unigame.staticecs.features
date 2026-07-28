namespace UniGame.StaticEcs.Features.Tests
{
    using System.Collections.Generic;
    using FFS.Libraries.StaticEcs;

    internal sealed class RecordingEffectHandler<TWorld, TEffect> : IEffectHandler<TWorld, TEffect>
        where TWorld : struct, IWorldType
        where TEffect : struct, IEffectType
    {
        public readonly List<(EntityGID Target, EntityGID Source, int Stacks, int PreviousStacks)> Applied = new();
        public readonly List<(EntityGID Target, EntityGID Source, int Stacks)> Ticks = new();
        public readonly List<(EntityGID Target, EntityGID Source, int Stacks, bool Expired)> Removed = new();

        public void OnApplied(EntityGID target, EntityGID source, int stacks, int previousStacks)
        {
            Applied.Add((target, source, stacks, previousStacks));
        }

        public void OnTick(EntityGID target, EntityGID source, int stacks)
        {
            Ticks.Add((target, source, stacks));
        }

        public void OnRemoved(EntityGID target, EntityGID source, int stacks, bool expired)
        {
            Removed.Add((target, source, stacks, expired));
        }

        public void Clear()
        {
            Applied.Clear();
            Ticks.Clear();
            Removed.Clear();
        }
    }
}
