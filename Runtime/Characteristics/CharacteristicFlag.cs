using System;

namespace UniGame.StaticEcs.Features {
    [Flags]
    public enum CharacteristicFlag : ulong {
        None               = 0,
        Health             = 1ul << 0,
        Mana               = 1ul << 1,
        Speed              = 1ul << 2,
        Shield             = 1ul << 3,
        Stun               = 1ul << 4,
        BlockChance        = 1ul << 5,
        DodgeChance        = 1ul << 6,
        ArmorResist        = 1ul << 7,
        CriticalChance     = 1ul << 8,
        CriticalMultiplier = 1ul << 9
    }
}
