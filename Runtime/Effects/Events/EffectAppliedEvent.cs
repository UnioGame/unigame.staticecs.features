namespace UniGame.StaticEcs.Features
{
    using FFS.Libraries.StaticEcs;

    /// <summary>
    /// Raised by <c>EffectOperations.Apply</c> for every application that takes effect: a new effect, a reapplication of
    /// a running one, and an instant effect. <see cref="PreviousStacks"/> tells them apart.
    /// </summary>
    public struct EffectAppliedEvent<TEffect> : IEvent
        where TEffect : struct, IEffectType
    {
        public EntityGID Source;
        public EntityGID Target;

        /// <summary>Stacks after the application.</summary>
        public int Stacks;

        /// <summary>Stacks of the running effect before the application; 0 for a new or an instant effect.</summary>
        public int PreviousStacks;

        public float TimeLeft;
    }
}
