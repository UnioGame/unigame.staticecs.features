namespace UniGame.StaticEcs.Features
{
    using FFS.Libraries.StaticEcs;

    /// <summary>Destroys the owner when its cooldown finishes, before any automatic restart.</summary>
    public struct CooldownDestroyOnCompleteTag : ITag
    {
    }
}
