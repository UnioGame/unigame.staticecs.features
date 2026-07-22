namespace UniGame.StaticEcs.Features
{
    using FFS.Libraries.StaticEcs;

    /// <summary>
    /// Set on a cast-entity to signal <c>AbilityStepProgressionSystem</c> that the current
    /// leaf has terminated and the cast is ready to advance. The terminal status is read from
    /// <see cref="AbilityStepStatusComponent"/>. Both the tag and the status component are removed
    /// by the progression system once it has consumed them.
    /// </summary>
    public struct AbilityStepReadyTag : ITag
    {
    }
}
