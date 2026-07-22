namespace UniGame.StaticEcs.Features
{
    using UniGame.Core.Runtime;
    using UniGame.StaticEcs.Unity;
    using UnityEngine;

    /// <summary>Creates a fresh Main-world damage pipeline feature.</summary>
    [CreateAssetMenu(
        menuName = "Static ECS/Features/Damage",
        fileName = nameof(DamageFeatureAsset)
    )]
    public sealed class DamageFeatureAsset : StaticEcsFeatureAsset
    {
        /// <summary>Whether the damage application system is installed.</summary>
        public bool registerApplySystem = true;

        /// <summary>Whether the default damage filter chain is installed.</summary>
        public bool registerDefaultChain = true;

        /// <summary>Execution order of damage application.</summary>
        public short applyOrder = DamageFeature.DefaultApplyOrder;

        /// <inheritdoc />
        public override IStaticEcsFeature<Main> CreateFeature(IContext context) =>
            new DamageFeature(registerApplySystem, registerDefaultChain, applyOrder);
    }
}
