using System;
using FFS.Libraries.StaticEcs;

namespace UniGame.StaticEcs.Features {
    [Serializable]
    public struct CharacteristicModifierEntry<TCharacteristic> : IMultiComponent
        where TCharacteristic : struct, ICharacteristicType {
        public EntityGIDCompact Source;
        public float Value;
        public CharacteristicModifierOp Op;
    }
}
