namespace UniGame.StaticEcs.Features
{
    using FFS.Libraries.StaticEcs;
    using UniCore.Runtime.ProfilerTools;

    /// <summary>
    /// Placeholder condition: never met. Warns once, because a <see cref="EffectActivationMode.Condition"/>
    /// effect with this condition never activates.
    /// </summary>
    public sealed class UnimplementedEffectCondition<TWorld, TEffect> : IEffectCondition<TWorld, TEffect>
        where TWorld : struct, IWorldType
        where TEffect : struct, IEffectType
    {
        private bool _warned;

        public bool IsMet(EntityGID target, EntityGID source)
        {
            if (_warned)
                return false;

            _warned = true;
            GameLog.LogWarning(
                $"[Effects] Condition activation is not implemented: {typeof(TEffect).Name} never activates.");
            return false;
        }
    }
}
