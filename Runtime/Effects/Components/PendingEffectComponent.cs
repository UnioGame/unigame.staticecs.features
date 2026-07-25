namespace UniGame.StaticEcs.Features
{
    using System;
    using FFS.Libraries.StaticEcs;

    /// <summary>Stores a scheduled effect before it becomes active.</summary>
    [Serializable]
    public struct PendingEffectComponent<TEffect> : IComponent
        where TEffect : struct, IEffectType
    {
        public EntityGIDCompact Source;
        public float DelayLeft;
        public float Duration;
        public float Period;
        public int Stacks;
    }
}
