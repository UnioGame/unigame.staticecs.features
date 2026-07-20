using System;
using FFS.Libraries.StaticEcs;
using UniGame.StaticEcs.Unity;
using UnityEngine;

namespace UniGame.StaticEcs.Features
{
    /// <summary>Applies all standard characteristic components from inline authoring data.</summary>
    [Serializable]
    public class AllCharacteristicsSerializableConverter<TWorld> : EcsSerializableConverter<TWorld>
        where TWorld : struct, IWorldType
    {
        /// <summary>Initial health settings.</summary>
        public CharacteristicSettings health = new CharacteristicSettings(100f, 0f, 100f);
        /// <summary>Initial mana settings.</summary>
        public CharacteristicSettings mana = new CharacteristicSettings(50f, 0f, 100f);
        /// <summary>Initial movement speed settings.</summary>
        public CharacteristicSettings speed = new CharacteristicSettings(5f, 0f, 20f);
        /// <summary>Initial shield settings.</summary>
        public CharacteristicSettings shield = new CharacteristicSettings(0f, 0f, 200f);
        /// <summary>Initial armor resistance settings.</summary>
        public CharacteristicSettings armorResist = new CharacteristicSettings(0f, 0f, 1f);
        /// <summary>Initial block chance settings.</summary>
        public CharacteristicSettings blockChance = new CharacteristicSettings(0f, 0f, 1f);
        /// <summary>Initial dodge chance settings.</summary>
        public CharacteristicSettings dodgeChance = new CharacteristicSettings(0f, 0f, 1f);
        /// <summary>Initial critical chance settings.</summary>
        public CharacteristicSettings critChance = new CharacteristicSettings(0f, 0f, 1f);
        /// <summary>Initial critical multiplier settings.</summary>
        public CharacteristicSettings critMultiplier = new CharacteristicSettings(2f, 1f, 10f);

        /// <inheritdoc />
        public override void Apply(World<TWorld>.Entity entity, GameObject host)
        {
            AllCharacteristicsConverter<TWorld>.ApplySettings(
                entity,
                health,
                mana,
                speed,
                shield,
                armorResist,
                blockChance,
                dodgeChance,
                critChance,
                critMultiplier);
        }
    }
}
