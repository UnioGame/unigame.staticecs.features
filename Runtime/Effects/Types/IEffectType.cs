namespace unigame.staticecs.features {
    /// <summary>
    /// Marker for effect-type structs registered through <c>EffectFeature&lt;TWorld, TEffect&gt;</c>.
    /// Effect types are pure compile-time tags; gameplay state lives in
    /// <see cref="EffectComponent{TEffect}"/> and per-effect data components.
    /// </summary>
    public interface IEffectType { }
}
