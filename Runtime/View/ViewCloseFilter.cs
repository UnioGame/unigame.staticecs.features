namespace UniGame.StaticEcs.Features
{
    using FFS.Libraries.StaticEcs;

    /// <summary>Selects ECS-managed views to close.</summary>
    public struct ViewCloseFilter
    {
        /// <summary>Matches views owned by this entity.</summary>
        public EntityGID Owner;

        /// <summary>Matches views synchronized from this entity.</summary>
        public EntityGID Source;

        /// <summary>Matches a View System identifier.</summary>
        public string ViewId;

        /// <summary>Matches a layout identifier.</summary>
        public string Layout;

        /// <summary>Matches every view when no narrower criteria are needed.</summary>
        public bool All;
    }
}
