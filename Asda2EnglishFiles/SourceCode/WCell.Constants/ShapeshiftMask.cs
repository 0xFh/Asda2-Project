using System;

namespace WCell.Constants
{
    [Flags]
    public enum ShapeshiftMask : uint
    {
        None = 0,
        Cat = 1,
        TreeOfLife = 2,
        Travel = 4,
        Aqua = 8,
        Bear = 16, // 0x00000010
        Ambient = 32, // 0x00000020
        Ghoul = 64, // 0x00000040
        DireBear = 128, // 0x00000080
        CreatureBear = 16384, // 0x00004000
        CreatureCat = 32768, // 0x00008000
        GhostWolf = 65536, // 0x00010000
        BattleStance = 131072, // 0x00020000
        DefensiveStance = 262144, // 0x00040000
        BerserkerStance = 524288, // 0x00080000
        EpicFlightForm = 134217728, // 0x08000000
        Shadow = 268435456, // 0x10000000
        Stealth = 536870912, // 0x20000000
        Moonkin = 1073741824, // 0x40000000
        SpiritOfRedemption = 2147483648, // 0x80000000
    }
}