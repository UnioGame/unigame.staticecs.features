namespace UniGame.StaticEcs.Features
{
    using FFS.Libraries.StaticEcs;

    /// <summary>
    /// Raised by <c>EffectOperations.Apply</c> when the target already has an active
    /// <see cref="EffectComponent{TEffect}"/> and the operation refreshes its lifetime
    /// and / or increments its stack count.
    /// </summary>
    public struct EffectRefreshedEvent<TEffect> : IEvent
        where TEffect : struct, IEffectType
    {
        public EntityGID Source;
        public EntityGID Target;
        public int Stacks;
        public int PreviousStacks;
        public float TimeLeft;
    }
}
