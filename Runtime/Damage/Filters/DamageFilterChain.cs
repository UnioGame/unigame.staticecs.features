using System.Collections.Generic;
using FFS.Libraries.StaticEcs;

namespace unigame.staticecs.features {
    /// <summary>
    /// Ordered set of <see cref="IDamageFilter{TWorld}"/> instances applied by
    /// <c>ApplyDamageSystem</c> to every <see cref="IncomingDamageEvent"/>. Registered as the
    /// <c>DamageFilterChain&lt;TWorld&gt;</c> world resource by <c>DamageFeature</c>; tests can
    /// replace it to drive a different chain.
    /// </summary>
    public sealed class DamageFilterChain<TWorld> : IResource
        where TWorld : struct, IWorldType {
        private readonly List<IDamageFilter<TWorld>> _filters;

        public DamageFilterChain() {
            _filters = new List<IDamageFilter<TWorld>>(8);
        }

        public DamageFilterChain(IEnumerable<IDamageFilter<TWorld>> filters) {
            _filters = new List<IDamageFilter<TWorld>>(filters);
        }

        public int Count => _filters.Count;

        public DamageFilterChain<TWorld> Add(IDamageFilter<TWorld> filter) {
            _filters.Add(filter);
            return this;
        }

        public DamageFilterChain<TWorld> Clear() {
            _filters.Clear();
            return this;
        }

        public void Apply(ref DamageContext ctx) {
            for (var i = 0; i < _filters.Count; i++) {
                _filters[i].Apply(ref ctx);
                if (ctx.Cancelled) {
                    return;
                }
            }
        }
    }
}
