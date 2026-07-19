using FFS.Libraries.StaticEcs;

namespace UniGame.StaticEcs.Features {
    public sealed class UnityAbilityRng<TWorld> : IAbilityRng<TWorld>
        where TWorld : struct, IWorldType {
        public int Range(int minInclusive, int maxExclusive) {
            return UnityEngine.Random.Range(minInclusive, maxExclusive);
        }

        public float Range(float minInclusive, float maxInclusive) {
            return UnityEngine.Random.Range(minInclusive, maxInclusive);
        }
    }
}
