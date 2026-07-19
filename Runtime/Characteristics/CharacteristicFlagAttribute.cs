using System;

namespace UniGame.StaticEcs.Features {
    [AttributeUsage(AttributeTargets.Struct, AllowMultiple = false, Inherited = false)]
    public sealed class CharacteristicFlagAttribute : Attribute {
        public CharacteristicFlag Flag { get; }

        public CharacteristicFlagAttribute(CharacteristicFlag flag) {
            Flag = flag;
        }
    }
}
