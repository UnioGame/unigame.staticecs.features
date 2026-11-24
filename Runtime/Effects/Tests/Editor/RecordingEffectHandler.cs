namespace UniGame.StaticEcs.Features.Tests
{
    using System.Collections.Generic;
    using FFS.Libraries.StaticEcs;

    internal sealed class RecordingEffectHandler<TWorld, TEffect> : IEffectHandler<TWorld, TEffect>
        where TWorld : struct, IWorldType
        where TEffect : struct, IEffectType
    {
        public readonly List<(EntityGID Target, EntityGID Source, int Stacks, int PreviousStacks)> Applied = new();
        public readonly List<float> Magnitudes = new();
        public readonly List<(EntityGID Target, EntityGID Source, int Stacks)> Ticks = new();

        public bool RejectApply;
        public readonly List<(int Stacks, int PreviousStacks)> Checked = new();
        public readonly List<(EntityGID Target, EntityGID Source, int Stacks, bool Expired)> Removed = new();

        public bool CanApply(in EffectApplyContext context)
        {
            Checked.Add((context.Stacks, context.PreviousStacks));
            return !RejectApply;
        }

        public void OnApplied(in EffectApplyContext context)
        {
            Applied.Add((context.Target, context.Source, context.Stacks, context.PreviousStacks));
            Magnitudes.Add(context.Magnitude);
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
            Checked.Clear();
            Magnitudes.Clear();
            Ticks.Clear();
            Removed.Clear();
        }
    }
}
