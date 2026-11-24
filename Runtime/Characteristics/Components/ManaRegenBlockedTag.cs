namespace UniGame.StaticEcs.Features
{
    using FFS.Libraries.StaticEcs;

    /// <summary>Suspends mana regeneration of the entity; the consumer sets and clears it.</summary>
    public struct ManaRegenBlockedTag : ITag { }
}
