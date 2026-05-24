using System;
using FFS.Libraries.StaticEcs;

namespace unigame.staticecs.features {
    [Serializable]
    public struct CharacteristicModifierEntry<TCharacteristic> : IMultiComponent
        where TCharacteristic : struct, ICharacteristicType {
        public EntityGIDCompact Source;
        public float Value;
        public CharacteristicModifierOp Op;
    }
}
