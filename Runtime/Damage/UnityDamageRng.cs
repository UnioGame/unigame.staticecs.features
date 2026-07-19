namespace UniGame.StaticEcs.Features {
    /// <summary>
    /// Default <see cref="IDamageRng"/> implementation backed by <see cref="Random"/>. Chosen
    /// when no other implementation has been registered before <c>DamageFeature.RegisterTypes</c>.
    /// </summary>
    public sealed class UnityDamageRng : IDamageRng {
        public float NextFloat01() {
            return UnityEngine.Random.value;
        }

        public bool RollChance(float chance01) {
            if (chance01 <= 0f) {
                return false;
            }

            if (chance01 >= 1f) {
                return true;
            }

            return UnityEngine.Random.value < chance01;
        }
    }
}
