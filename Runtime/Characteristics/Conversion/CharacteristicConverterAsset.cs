using FFS.Libraries.StaticEcs;
 
using UnityEngine;

namespace UniGame.StaticEcs.Features {
    using Unity;

    public abstract class CharacteristicConverterAsset<TWorld, TCharacteristic> :
        EcsConverterAsset<TWorld>
        where TWorld : struct, IWorldType
        where TCharacteristic : struct, ICharacteristicType {
        [SerializeField] protected float _value;
        [SerializeField] protected float _minValue;
        [SerializeField] protected float _maxValue = 100f;

        public sealed override void Apply(World<TWorld>.Entity entity, GameObject host) {
            entity.Set(new CharacteristicComponent<TCharacteristic>(_value, _minValue, _maxValue, _value));
        }
    }
}
