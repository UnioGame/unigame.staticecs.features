namespace UniGame.StaticEcs.Features
{
    using FFS.Libraries.StaticEcs;

    /// <summary>Owns the monotonically increasing view request sequence for one world.</summary>
    internal struct ViewKeySequenceResource<TWorld> : IResource
        where TWorld : struct, IWorldType
    {
        internal ulong next;

        internal ViewKey Next()
        {
            next++;
            if (next == 0)
                next++;

            return new ViewKey(next);
        }
    }
}
