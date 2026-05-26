using FFS.Libraries.StaticEcs;

namespace unigame.staticecs.features {
    public interface IAbilityRng<TWorld> : IResource
        where TWorld : struct, IWorldType {
        int Range(int minInclusive, int maxExclusive);
        float Range(float minInclusive, float maxInclusive);
    }
}
