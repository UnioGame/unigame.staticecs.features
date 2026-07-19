using System;
using FFS.Libraries.StaticEcs;

namespace UniGame.StaticEcs.Features {
    [Serializable]
    [CharacteristicFlag(CharacteristicFlag.Stun)]
    public struct StunSource : IMultiComponent {
        public EntityGIDCompact Source;
    }
}
