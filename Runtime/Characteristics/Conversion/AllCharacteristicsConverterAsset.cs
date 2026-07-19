using FFS.Libraries.StaticEcs;
using UnityEngine;

namespace UniGame.StaticEcs.Features
{
    using Unity;

    /// <summary>Main-world alias for <see cref="AllCharacteristicsConverterAsset{TWorld}"/>.</summary>
    [CreateAssetMenu(menuName = "Static ECS/Characteristics/All Characteristics Converter")]
    public sealed class AllCharacteristicsConverterAsset : AllCharacteristicsConverterAsset<Main>
    {
    }

    /// <summary>
    /// ScriptableObject converter that applies all nine standard characteristic components
    /// to an entity in a single step. Place in <c>assetConverters</c> on an <see cref="EcsEntityProvider{TWorld}"/>.
    /// </summary>
    public class AllCharacteristicsConverterAsset<TWorld> : EcsConverterAsset<TWorld>
        where TWorld : struct, IWorldType
    {
        [SerializeField]
        public CharacteristicSettings health = new CharacteristicSettings(100f, 0f, 100f);

        [SerializeField]
        public CharacteristicSettings mana = new CharacteristicSettings(50f, 0f, 100f);

        [SerializeField]
        public CharacteristicSettings speed = new CharacteristicSettings(5f, 0f, 20f);

        [SerializeField]
        public CharacteristicSettings shield = new CharacteristicSettings(0f, 0f, 200f);

        [SerializeField]
        public CharacteristicSettings armorResist = new CharacteristicSettings(0f, 0f, 1f);

        [SerializeField]
        public CharacteristicSettings blockChance = new CharacteristicSettings(0f, 0f, 1f);

        [SerializeField]
        public CharacteristicSettings dodgeChance = new CharacteristicSettings(0f, 0f, 1f);

        [SerializeField]
        public CharacteristicSettings critChance = new CharacteristicSettings(0f, 0f, 1f);

        [SerializeField]
        public CharacteristicSettings critMultiplier = new CharacteristicSettings(2f, 1f, 10f);

        /// <inheritdoc/>
        public override void Apply(World<TWorld>.Entity entity, GameObject host)
        {
            entity.Set(new CharacteristicComponent<HealthCharacteristic>(health.value, health.min, health.max,
                health.value));
            entity.Set(new CharacteristicComponent<ManaCharacteristic>(mana.value, mana.min, mana.max, mana.value));
            entity.Set(new CharacteristicComponent<SpeedCharacteristic>(speed.value, speed.min, speed.max,
                speed.value));
            entity.Set(new CharacteristicComponent<ShieldCharacteristic>(shield.value, shield.min, shield.max,
                shield.value));
            entity.Set(new CharacteristicComponent<ArmorResistCharacteristic>(armorResist.value, armorResist.min,
                armorResist.max, armorResist.value));
            entity.Set(new CharacteristicComponent<BlockChanceCharacteristic>(blockChance.value, blockChance.min,
                blockChance.max, blockChance.value));
            entity.Set(new CharacteristicComponent<DodgeChanceCharacteristic>(dodgeChance.value, dodgeChance.min,
                dodgeChance.max, dodgeChance.value));
            entity.Set(new CharacteristicComponent<CriticalChanceCharacteristic>(critChance.value, critChance.min,
                critChance.max, critChance.value));
            entity.Set(new CharacteristicComponent<CriticalMultiplierCharacteristic>(critMultiplier.value,
                critMultiplier.min, critMultiplier.max, critMultiplier.value));
        }
    }
}