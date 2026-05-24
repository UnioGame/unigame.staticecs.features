using System;
using FFS.Libraries.StaticEcs;

namespace unigame.staticecs.features {
    [Serializable]
    public struct ManaRegenComponent : IComponent {
        public float Rate;
    }
}
