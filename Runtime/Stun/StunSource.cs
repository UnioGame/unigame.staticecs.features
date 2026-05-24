using System;
using FFS.Libraries.StaticEcs;

namespace unigame.staticecs.features {
    [Serializable]
    [CharacteristicFlag(CharacteristicFlag.Stun)]
    public struct StunSource : IMultiComponent {
        public EntityGIDCompact Source;
    }
}
