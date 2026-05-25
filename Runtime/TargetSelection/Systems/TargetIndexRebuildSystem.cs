using FFS.Libraries.StaticEcs;

namespace unigame.staticecs.features {
    /// <summary>
    /// Rebuilds the world's <see cref="ITargetIndex{TWorld}"/> every Update tick. v1 strategy is
    /// "rebuild always" — cheap enough at sandbox scale and correct under entity churn. Replace
    /// with an incremental rebuild once entity count grows beyond a few hundred.
    /// </summary>
    public sealed class TargetIndexRebuildSystem<TWorld> : ISystem
        where TWorld : struct, IWorldType {
        public void Update() {
            if (!World<TWorld>.HasResource<ITargetIndex<TWorld>>()) {
                return;
            }
            World<TWorld>.GetResource<ITargetIndex<TWorld>>().Rebuild();
        }
    }
}
