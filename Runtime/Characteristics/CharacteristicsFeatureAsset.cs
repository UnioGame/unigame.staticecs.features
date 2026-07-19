using UniGame.Core.Runtime;
using UniGame.StaticEcs.Unity;
using UnityEngine;

namespace UniGame.StaticEcs.Features
{
    /// <summary>Creates a fresh Main-world standard characteristics feature.</summary>
    [CreateAssetMenu(menuName = "Static ECS/Features/Characteristics", fileName = nameof(CharacteristicsFeatureAsset))]
    public sealed class CharacteristicsFeatureAsset : StaticEcsFeatureAsset
    {
        /// <summary>Whether mana regeneration is installed in the update group.</summary>
        public bool registerManaRegen = true;
        /// <summary>Execution order of mana regeneration.</summary>
        public short manaRegenOrder = ManaFeature.DefaultRegenOrder;

        /// <inheritdoc />
        public override IStaticEcsFeature<Main> CreateFeature(IContext context)
        {
            return new CharacteristicsFeature(registerManaRegen, manaRegenOrder);
        }
    }
}
