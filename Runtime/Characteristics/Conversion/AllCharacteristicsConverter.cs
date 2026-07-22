namespace UniGame.StaticEcs.Features
{
    using FFS.Libraries.StaticEcs;
    using Unity;
    using UnityEngine;

    /// <summary>Main-world alias for <see cref="AllCharacteristicsConverter{TWorld}"/>.</summary>
    [AddComponentMenu("Static ECS/Characteristics/All Characteristics Converter")]
    public sealed class AllCharacteristicsConverter : AllCharacteristicsConverter<Main> { }

    /// <summary>
    /// MonoBehaviour converter that applies all nine standard characteristic components
    /// to an entity in a single step. Attach to a GameObject alongside <see cref="EcsEntityProvider{TWorld}"/>,
    /// or use <see cref="ApplySettings"/> for entities created directly from code.
    /// </summary>
    public class AllCharacteristicsConverter<TWorld> : EcsMonoConverter<TWorld>
        where TWorld : struct, IWorldType
    {
        [SerializeField]
        public CharacteristicSettings Health = new CharacteristicSettings(100f, 0f, 100f);

        [SerializeField]
        public CharacteristicSettings Mana = new CharacteristicSettings(50f, 0f, 100f);

        [SerializeField]
        public CharacteristicSettings Speed = new CharacteristicSettings(5f, 0f, 20f);

        [SerializeField]
        public CharacteristicSettings Shield = new CharacteristicSettings(0f, 0f, 200f);

        [SerializeField]
        public CharacteristicSettings ArmorResist = new CharacteristicSettings(0f, 0f, 1f);

        [SerializeField]
        public CharacteristicSettings BlockChance = new CharacteristicSettings(0f, 0f, 1f);

        [SerializeField]
        public CharacteristicSettings DodgeChance = new CharacteristicSettings(0f, 0f, 1f);

        [SerializeField]
        public CharacteristicSettings CritChance = new CharacteristicSettings(0f, 0f, 1f);

        [SerializeField]
        public CharacteristicSettings CritMultiplier = new CharacteristicSettings(2f, 1f, 10f);

        /// <summary>Applies all nine standard characteristic settings without creating a MonoBehaviour.</summary>
        public static void ApplySettings(
            World<TWorld>.Entity entity,
            CharacteristicSettings health,
            CharacteristicSettings mana,
            CharacteristicSettings speed,
            CharacteristicSettings shield,
            CharacteristicSettings armorResist,
            CharacteristicSettings blockChance,
            CharacteristicSettings dodgeChance,
            CharacteristicSettings critChance,
            CharacteristicSettings critMultiplier
        )
        {
            entity.Set(
                new CharacteristicComponent<HealthCharacteristic>(
                    health.value,
                    health.min,
                    health.max,
                    health.value
                )
            );
            entity.Set(
                new CharacteristicComponent<ManaCharacteristic>(
                    mana.value,
                    mana.min,
                    mana.max,
                    mana.value
                )
            );
            entity.Set(
                new CharacteristicComponent<SpeedCharacteristic>(
                    speed.value,
                    speed.min,
                    speed.max,
                    speed.value
                )
            );
            entity.Set(
                new CharacteristicComponent<ShieldCharacteristic>(
                    shield.value,
                    shield.min,
                    shield.max,
                    shield.value
                )
            );
            entity.Set(
                new CharacteristicComponent<ArmorResistCharacteristic>(
                    armorResist.value,
                    armorResist.min,
                    armorResist.max,
                    armorResist.value
                )
            );
            entity.Set(
                new CharacteristicComponent<BlockChanceCharacteristic>(
                    blockChance.value,
                    blockChance.min,
                    blockChance.max,
                    blockChance.value
                )
            );
            entity.Set(
                new CharacteristicComponent<DodgeChanceCharacteristic>(
                    dodgeChance.value,
                    dodgeChance.min,
                    dodgeChance.max,
                    dodgeChance.value
                )
            );
            entity.Set(
                new CharacteristicComponent<CriticalChanceCharacteristic>(
                    critChance.value,
                    critChance.min,
                    critChance.max,
                    critChance.value
                )
            );
            entity.Set(
                new CharacteristicComponent<CriticalMultiplierCharacteristic>(
                    critMultiplier.value,
                    critMultiplier.min,
                    critMultiplier.max,
                    critMultiplier.value
                )
            );
        }

        /// <inheritdoc/>
        public override void Apply(World<TWorld>.Entity entity, GameObject host)
        {
            ApplySettings(
                entity,
                Health,
                Mana,
                Speed,
                Shield,
                ArmorResist,
                BlockChance,
                DodgeChance,
                CritChance,
                CritMultiplier
            );
        }
    }
}
