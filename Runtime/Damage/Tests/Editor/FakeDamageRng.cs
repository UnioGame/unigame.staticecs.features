namespace UniGame.StaticEcs.Features.Tests
{
    internal sealed class FakeDamageRng : IDamageRng
    {
        public bool NextRoll;
        public float NextValue;

        public float NextFloat01() => NextValue;

        public bool RollChance(float chance01) => NextRoll;
    }
}
