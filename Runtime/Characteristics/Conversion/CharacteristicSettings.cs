using System;

namespace unigame.staticecs.features {
    /// <summary>Value, min, and max settings for a single characteristic stat used by group converters.</summary>
    [Serializable]
    public struct CharacteristicSettings {
        /// <summary>Initial and base value of the characteristic.</summary>
        public float value;
        /// <summary>Minimum allowed value.</summary>
        public float min;
        /// <summary>Maximum allowed value.</summary>
        public float max;

        /// <summary>Creates a new <see cref="CharacteristicSettings"/> with the given bounds.</summary>
        public CharacteristicSettings(float value, float min, float max) {
            this.value = value;
            this.min   = min;
            this.max   = max;
        }
    }
}
