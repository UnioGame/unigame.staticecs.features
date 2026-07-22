namespace UniGame.StaticEcs.Features
{
    using FFS.Libraries.StaticEcs;
    using Unity;
    using UnityEngine;

    public abstract class CharacteristicConverter<TWorld, TCharacteristic>
        : EcsMonoConverter<TWorld, CharacteristicComponent<TCharacteristic>>
        where TWorld : struct, IWorldType
        where TCharacteristic : struct, ICharacteristicType
    {
        [SerializeField]
        protected float _value;

        [SerializeField]
        protected float _minValue;

        [SerializeField]
        protected float _maxValue = 100f;

        protected override CharacteristicComponent<TCharacteristic> Build(GameObject host)
        {
            return CharacteristicConverterUtility.Build<TCharacteristic>(
                new CharacteristicSettings(_value, _minValue, _maxValue)
            );
        }
    }

    internal static class CharacteristicConverterUtility
    {
        public static CharacteristicComponent<TCharacteristic> Build<TCharacteristic>(
            CharacteristicSettings settings
        )
            where TCharacteristic : struct, ICharacteristicType
        {
            return new CharacteristicComponent<TCharacteristic>(
                settings.value,
                settings.min,
                settings.max,
                settings.value
            );
        }
    }
}
