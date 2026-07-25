namespace UniGame.StaticEcs.Features
{
    using UniGame.StaticEcs.Unity;

    /// <summary>Default-world tick system for one effect type.</summary>
    public sealed class EffectTickSystem<TEffect> : EffectTickSystem<Main, TEffect>
        where TEffect : struct, IEffectType { }
}
