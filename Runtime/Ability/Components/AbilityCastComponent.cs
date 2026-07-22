namespace UniGame.StaticEcs.Features
{
    using FFS.Libraries.StaticEcs;

    /// <summary>
    /// Per cast-entity state. Lives on a dedicated cast-entity created by
    /// <c>AbilityCastSystem</c> and supports foreground, channel, and branch casts concurrently.
    ///
    /// <see cref="RootEntered"/> is false until the progression system descends into the root
    /// step; once set, an empty stack means the cast has completed.
    /// </summary>
    public struct AbilityCastComponent : IComponent
    {
        /// <summary>The ability being executed.</summary>
        public AbilityId AbilityId;

        /// <summary>The entity performing the cast.</summary>
        public EntityGID Caster;

        /// <summary>The primary target selected for the cast.</summary>
        public EntityGID PrimaryTarget;

        /// <summary>Whether progression has entered the root step.</summary>
        public bool RootEntered;
    }
}
