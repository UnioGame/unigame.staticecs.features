using FFS.Libraries.StaticEcs;

namespace unigame.staticecs.features {
    /// <summary>
    /// One step of the damage pipeline. Filters are stateless and run in registration order on
    /// a shared <see cref="DamageContext"/>. A filter that sets <see cref="DamageContext.Cancelled"/>
    /// terminates the chain; later filters and the apply step are skipped.
    /// </summary>
    /// <typeparam name="TWorld">World type whose entities the filter resolves against.</typeparam>
    public interface IDamageFilter<TWorld>
        where TWorld : struct, IWorldType {
        void Apply(ref DamageContext ctx);
    }
}
