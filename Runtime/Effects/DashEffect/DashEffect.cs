namespace UniGame.StaticEcs.Features
{
    /// <summary>
    /// Marker for a short displacement of the target along the ground plane. <see cref="DashEffectHandler{TWorld}"/>
    /// turns the application into a <see cref="DashComponent"/> that the movement of the project consumes, and the
    /// effect itself stays on the target for its duration so that a second dash cannot start meanwhile.
    /// </summary>
    [EffectFlag(EffectFlag.Dash)]
    public struct DashEffect : IEffectType { }
}
