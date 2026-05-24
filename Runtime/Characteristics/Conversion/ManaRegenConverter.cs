using FFS.Libraries.StaticEcs;
using unigame.staticecs.unity;
using UnityEngine;

namespace unigame.staticecs.features {
    [AddComponentMenu("Static ECS/Characteristics/Mana Regen Converter")]
    public sealed class ManaRegenConverter : EcsValueConverter<Main, ManaRegenComponent, float> {
        protected override ManaRegenComponent Convert(GameObject host, float value) {
            return new ManaRegenComponent { Rate = value };
        }
    }
}
