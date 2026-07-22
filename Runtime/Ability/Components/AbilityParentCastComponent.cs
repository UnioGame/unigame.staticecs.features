namespace UniGame.StaticEcs.Features
{
    using FFS.Libraries.StaticEcs;

    /// <summary>
    /// Reference from a sub-ability cast-entity to its parent. Absent on root casts and used
    /// to cascade cancellation from a parent cast to its children.
    /// </summary>
    public struct AbilityParentCastComponent : IComponent
    {
        /// <summary>The parent cast entity.</summary>
        public EntityGID Parent;
    }
}
