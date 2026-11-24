namespace UniGame.StaticEcs.Features
{
    using FFS.Libraries.StaticEcs;

    /// <summary>
    /// One application of an effect to one target, handed to <see cref="IEffectHandler{TWorld,TEffect}"/>.
    /// </summary>
    public readonly struct EffectApplyContext
    {
        public readonly EntityGID Target;
        public readonly EntityGID Source;

        /// <summary>
        /// Stacks after the application. <see cref="IEffectHandler{TWorld,TEffect}.CanApply"/> sees the stacks
        /// currently on the target, because the new total is not decided yet.
        /// </summary>
        public readonly int Stacks;

        /// <summary>Stacks on the target before the application; 0 for a new or an instant effect.</summary>
        public readonly int PreviousStacks;

        /// <summary>Strength given at the first application, for example damage or heal amount.</summary>
        public readonly float Magnitude;

        /// <summary>Readable name of the source captured for history.</summary>
        public readonly string SourceName;

        public EffectApplyContext(
            EntityGID target,
            EntityGID source,
            int stacks,
            int previousStacks,
            float magnitude,
            string sourceName)
        {
            Target = target;
            Source = source;
            Stacks = stacks;
            PreviousStacks = previousStacks;
            Magnitude = magnitude;
            SourceName = sourceName;
        }
    }
}
