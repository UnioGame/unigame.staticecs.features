using System;
using FFS.Libraries.StaticEcs;

namespace UniGame.StaticEcs.Features {
    [Serializable]
    public struct ModifierBackRef : IMultiComponent {
        public EntityGIDCompact Target;
        public CharacteristicFlag StatMask;
    }
}
