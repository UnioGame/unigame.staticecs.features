using FFS.Libraries.StaticEcs;
 
using UnityEngine;

namespace UniGame.StaticEcs.Features {
    using Unity;

    [AddComponentMenu("Static ECS/Characteristics/Mana Regen Converter")]
    public sealed class ManaRegenConverter : EcsValueConverter<Main, ManaRegenComponent, float> {
        protected override ManaRegenComponent Convert(GameObject host, float value) {
            return new ManaRegenComponent { Rate = value };
        }
    }
}
