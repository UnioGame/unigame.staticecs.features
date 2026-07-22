namespace UniGame.StaticEcs.Features
{
    using UniGame.Core.Runtime;
    using UniGame.StaticEcs.Unity;
    using UnityEngine;

    /// <summary>Creates a fresh Main-world stun state feature.</summary>
    [CreateAssetMenu(menuName = "Static ECS/Features/Stun", fileName = nameof(StunFeatureAsset))]
    public sealed class StunFeatureAsset : StaticEcsFeatureAsset
    {
        /// <inheritdoc />
        public override IStaticEcsFeature<Main> CreateFeature(IContext context) =>
            new StunFeature();
    }
}
