namespace UniGame.StaticEcs.Features.Tests
{
    public sealed class FakeAbilityRng : IAbilityRng<TestAbilityWorld>
    {
        public int NextInt;
        public float NextFloat;

        public int Range(int minInclusive, int maxExclusive)
        {
            return NextInt;
        }

        public float Range(float minInclusive, float maxInclusive)
        {
            return NextFloat;
        }
    }
}
