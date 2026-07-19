using FFS.Libraries.StaticEcs;

namespace UniGame.StaticEcs.Features {
    /// <summary>
    /// Random source used by damage filters for probabilistic rolls. Registered as the
    /// <see cref="IDamageRng"/>-typed resource on the world by <c>DamageFeature</c>; tests inject
    /// deterministic implementations to drive filter behavior reliably.
    /// </summary>
    public interface IDamageRng : IResource {
        /// <summary>Returns a value in the inclusive range [0, 1].</summary>
        float NextFloat01();

        /// <summary>Returns true when a fresh roll succeeds against the given 0..1 probability.</summary>
        bool RollChance(float chance01);
    }
}
