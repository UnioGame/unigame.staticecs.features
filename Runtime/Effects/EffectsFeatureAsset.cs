using UniGame.Core.Runtime;
using UniGame.StaticEcs.Unity;
using UnityEngine;

namespace UniGame.StaticEcs.Features
{
    /// <summary>Creates a fresh Main-world standard effects feature.</summary>
    [CreateAssetMenu(menuName = "Static ECS/Features/Effects", fileName = nameof(EffectsFeatureAsset))]
    public sealed class EffectsFeatureAsset : StaticEcsFeatureAsset
    {
        /// <inheritdoc />
        public override IStaticEcsFeature<Main> CreateFeature(IContext context) => new EffectsFeature();
    }
}
