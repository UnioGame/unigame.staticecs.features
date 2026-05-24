using System;

namespace unigame.staticecs.features {
    [Flags]
    public enum CharacteristicFlag : ulong {
        None   = 0,
        Health = 1ul << 0,
        Mana   = 1ul << 1,
        Speed  = 1ul << 2,
        Shield = 1ul << 3,
        Stun   = 1ul << 4
    }
}
