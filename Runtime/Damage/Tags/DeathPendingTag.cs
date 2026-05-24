using FFS.Libraries.StaticEcs;

namespace unigame.staticecs.features {
    /// <summary>
    /// Set by the apply step when an entity reaches zero health. The death slice consumes this
    /// tag to perform cleanup and entity destruction in a separate frame.
    /// </summary>
    public struct DeathPendingTag : ITag { }
}
