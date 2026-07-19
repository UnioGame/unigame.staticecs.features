using FFS.Libraries.StaticEcs;
 
using UnityEngine;

namespace UniGame.StaticEcs.Features {
    using Unity;

    /// <summary>Main-world alias for <see cref="AllCharacteristicsConverter{TWorld}"/>.</summary>
    [AddComponentMenu("Static ECS/Characteristics/All Characteristics Converter")]
    public sealed class AllCharacteristicsConverter : AllCharacteristicsConverter<Main> { }

    /// <summary>
    /// MonoBehaviour converter that applies all nine standard characteristic components
    /// to an entity in a single step. Attach to a GameObject alongside <see cref="EcsEntityProvider{TWorld}"/>,
    /// or instantiate in code and call <see cref="Apply"/> directly.
    /// </summary>
    public class AllCharacteristicsConverter<TWorld> : EcsMonoConverter<TWorld>
        where TWorld : struct, IWorldType {

        [SerializeField] public CharacteristicSettings Health          = new CharacteristicSettings(100f, 0f, 100f);
        [SerializeField] public CharacteristicSettings Mana            = new CharacteristicSettings(50f,  0f, 100f);
        [SerializeField] public CharacteristicSettings Speed           = new CharacteristicSettings(5f,   0f, 20f);
        [SerializeField] public CharacteristicSettings Shield          = new CharacteristicSettings(0f,   0f, 200f);
        [SerializeField] public CharacteristicSettings ArmorResist     = new CharacteristicSettings(0f,   0f, 1f);
        [SerializeField] public CharacteristicSettings BlockChance     = new CharacteristicSettings(0f,   0f, 1f);
        [SerializeField] public CharacteristicSettings DodgeChance     = new CharacteristicSettings(0f,   0f, 1f);
        [SerializeField] public CharacteristicSettings CritChance      = new CharacteristicSettings(0f,   0f, 1f);
        [SerializeField] public CharacteristicSettings CritMultiplier  = new CharacteristicSettings(2f,   1f, 10f);

        /// <inheritdoc/>
        public override void Apply(World<TWorld>.Entity entity, GameObject host) {
            entity.Set(new CharacteristicComponent<HealthCharacteristic>(Health.value,         Health.min,         Health.max,         Health.value));
            entity.Set(new CharacteristicComponent<ManaCharacteristic>(Mana.value,             Mana.min,           Mana.max,           Mana.value));
            entity.Set(new CharacteristicComponent<SpeedCharacteristic>(Speed.value,           Speed.min,          Speed.max,          Speed.value));
            entity.Set(new CharacteristicComponent<ShieldCharacteristic>(Shield.value,         Shield.min,         Shield.max,         Shield.value));
            entity.Set(new CharacteristicComponent<ArmorResistCharacteristic>(ArmorResist.value,   ArmorResist.min,    ArmorResist.max,    ArmorResist.value));
            entity.Set(new CharacteristicComponent<BlockChanceCharacteristic>(BlockChance.value,   BlockChance.min,    BlockChance.max,    BlockChance.value));
            entity.Set(new CharacteristicComponent<DodgeChanceCharacteristic>(DodgeChance.value,   DodgeChance.min,    DodgeChance.max,    DodgeChance.value));
            entity.Set(new CharacteristicComponent<CriticalChanceCharacteristic>(CritChance.value, CritChance.min,     CritChance.max,     CritChance.value));
            entity.Set(new CharacteristicComponent<CriticalMultiplierCharacteristic>(CritMultiplier.value, CritMultiplier.min, CritMultiplier.max, CritMultiplier.value));
        }
    }
}
