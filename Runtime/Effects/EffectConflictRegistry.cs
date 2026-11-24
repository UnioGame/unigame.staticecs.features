namespace UniGame.StaticEcs.Features
{
    using System.Collections.Generic;
    using FFS.Libraries.StaticEcs;

    /// <summary>Conflict group and priority of one effect type.</summary>
    public readonly struct EffectConflictEntry
    {
        public readonly string Group;
        public readonly int Priority;
        public readonly EffectFlag Flag;

        public EffectConflictEntry(string group, int priority, EffectFlag flag)
        {
            Group = group;
            Priority = priority;
            Flag = flag;
        }
    }

    /// <summary>
    /// Per-world resource that maps effect ids to their conflict group. Effects of one group exclude each
    /// other on a target; filled by <c>EffectFeature</c> for types that declare a group.
    /// </summary>
    public sealed class EffectConflictRegistry : IResource
    {
        private readonly Dictionary<int, EffectConflictEntry> _entries = new();

        public void Register(EffectId id, string group, int priority, EffectFlag flag)
        {
            _entries[id.Value] = new EffectConflictEntry(group, priority, flag);
        }

        public bool TryGet(EffectId id, out EffectConflictEntry entry)
        {
            return _entries.TryGetValue(id.Value, out entry);
        }
    }
}
