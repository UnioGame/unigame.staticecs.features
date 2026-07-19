 
using UnityEngine;

namespace UniGame.StaticEcs.Features {
    using Unity;

    [AddComponentMenu("Static ECS/Characteristics/Health Converter")]
    public sealed class HealthConverter : CharacteristicConverter<Main, HealthCharacteristic> { }
}
