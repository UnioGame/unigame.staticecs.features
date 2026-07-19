using UniGame.Core.Runtime;
using UniGame.StaticEcs.Unity;
using UnityEngine;

namespace UniGame.StaticEcs.Features
{
    /// <summary>Creates a fresh Main-world movement data feature.</summary>
    [CreateAssetMenu(menuName = "Static ECS/Features/Movement", fileName = nameof(MovementFeatureAsset))]
    public sealed class MovementFeatureAsset : StaticEcsFeatureAsset
    {
        /// <inheritdoc />
        public override IStaticEcsFeature<Main> CreateFeature(IContext context) => new MovementFeature();
    }
}
