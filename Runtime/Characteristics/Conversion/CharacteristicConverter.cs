using FFS.Libraries.StaticEcs;
 
using UnityEngine;

namespace UniGame.StaticEcs.Features {
    using Unity;

    public abstract class CharacteristicConverter<TWorld, TCharacteristic> :
        EcsMonoConverter<TWorld, CharacteristicComponent<TCharacteristic>>
        where TWorld : struct, IWorldType
        where TCharacteristic : struct, ICharacteristicType {
        [SerializeField] protected float _value;
        [SerializeField] protected float _minValue;
        [SerializeField] protected float _maxValue = 100f;

        protected override CharacteristicComponent<TCharacteristic> Build(GameObject host) {
            return new CharacteristicComponent<TCharacteristic>(_value, _minValue, _maxValue, _value);
        }
    }
}
