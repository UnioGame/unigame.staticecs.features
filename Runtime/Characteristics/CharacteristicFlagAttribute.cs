using System;

namespace unigame.staticecs.features {
    [AttributeUsage(AttributeTargets.Struct, AllowMultiple = false, Inherited = false)]
    public sealed class CharacteristicFlagAttribute : Attribute {
        public CharacteristicFlag Flag { get; }

        public CharacteristicFlagAttribute(CharacteristicFlag flag) {
            Flag = flag;
        }
    }
}
