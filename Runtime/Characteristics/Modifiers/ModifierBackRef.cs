using System;
using FFS.Libraries.StaticEcs;

namespace unigame.staticecs.features {
    [Serializable]
    public struct ModifierBackRef : IMultiComponent {
        public EntityGIDCompact Target;
        public CharacteristicFlag StatMask;
    }
}
