using FFS.Libraries.StaticEcs;
 
using UnityEngine;

namespace UniGame.StaticEcs.Features {
    using Unity;

    [AddComponentMenu("Static ECS/Characteristics/Mana Regen Converter")]
    public sealed class ManaRegenConverter : EcsValueConverter<Main, ManaRegenComponent, float> {
        protected override ManaRegenComponent Convert(GameObject host, float value) {
            return ManaRegenConverterUtility.Build(value);
        }
    }

    /// <summary>Creates a mana regeneration component from inline authoring data.</summary>
    [System.Serializable]
    public sealed class ManaRegenSerializableConverter : EcsComponentSerializableConverter<ManaRegenComponent> {
        [SerializeField]
        private float _rate;

        /// <summary>Gets or sets the mana restored per tick.</summary>
        public float Rate {
            get => _rate;
            set => _rate = value;
        }

        protected override ManaRegenComponent Build(GameObject host) {
            return ManaRegenConverterUtility.Build(_rate);
        }
    }

    internal static class ManaRegenConverterUtility {
        public static ManaRegenComponent Build(float rate) {
            return new ManaRegenComponent { Rate = rate };
        }
    }
}
