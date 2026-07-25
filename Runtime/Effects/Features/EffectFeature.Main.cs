namespace UniGame.StaticEcs.Features
{
    using UniGame.StaticEcs.Unity;

    /// <summary>Default-world generic feature for one effect type.</summary>
    public class EffectFeature<TEffect> : EffectFeature<Main, TEffect>
        where TEffect : struct, IEffectType { }
}
