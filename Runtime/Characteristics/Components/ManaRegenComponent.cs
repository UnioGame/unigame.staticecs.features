using System;
using FFS.Libraries.StaticEcs;

namespace UniGame.StaticEcs.Features {
    [Serializable]
    public struct ManaRegenComponent : IComponent {
        public float Rate;
    }
}
