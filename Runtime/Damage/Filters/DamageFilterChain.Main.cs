namespace UniGame.StaticEcs.Features
{
    using System.Collections.Generic;
    using UniGame.StaticEcs.Unity;

    /// <summary>Main-world ordered damage filter chain.</summary>
    public sealed class DamageFilterChain : DamageFilterChain<Main>
    {
        /// <summary>Creates an empty Main-world damage filter chain.</summary>
        public DamageFilterChain() { }

        /// <summary>Creates a Main-world damage filter chain from ordered filters.</summary>
        public DamageFilterChain(IEnumerable<IDamageFilter<Main>> filters)
            : base(filters) { }
    }
}
