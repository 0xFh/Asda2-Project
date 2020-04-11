using System;

namespace WCell.Constants.Spells
{
    [Flags]
    public enum SpellAttributesExD : uint
    {
        None = 0,
        AttrExD_0_0x1 = 1,
        NoReagentsInPrep = 2,
        AttrExD_2_0x4 = 4,
        UsableWhileStunned = 8,
        AttrExD_4_0x10 = 16, // 0x00000010
        SingleTargetOnly = 32, // 0x00000020
        AttrExD_6_0x40 = 64, // 0x00000040
        AttrExD_7_0x80 = 128, // 0x00000080
        AttrExD_8_0x100 = 256, // 0x00000100
        AttrExD_9_0x200 = 512, // 0x00000200
        AttrExD_10_0x400 = 1024, // 0x00000400
        CannotBeAbsorbed = 2048, // 0x00000800
        AttrExD_12_0x1000 = 4096, // 0x00001000
        AttrExD_13_0x2000 = 8192, // 0x00002000
        AttrExD_14_0x4000 = 16384, // 0x00004000
        AttrExD_15_0x8000 = 32768, // 0x00008000
        AttrExD_16_0x10000 = 65536, // 0x00010000
        UsableWhileFeared = 131072, // 0x00020000
        UsableWhileConfused = 262144, // 0x00040000
        AttrExD_19_0x80000 = 524288, // 0x00080000
        AttrExD_20_0x100000 = 1048576, // 0x00100000
        AttrExD_21_0x200000 = 2097152, // 0x00200000
        AttrExD_22_0x400000 = 4194304, // 0x00400000
        AttrExD_23_0x800000 = 8388608, // 0x00800000
        AttrExD_24_0x1000000 = 16777216, // 0x01000000
        AttrExD_25_0x2000000 = 33554432, // 0x02000000
        AttrExD_26_0x4000000 = 67108864, // 0x04000000
        AttrExD_27_0x8000000 = 134217728, // 0x08000000
        AttrExD_28_0x10000000 = 268435456, // 0x10000000
        AttrExD_29_0x20000000 = 536870912, // 0x20000000
        AttrExD_30_0x40000000 = 1073741824, // 0x40000000
        AttrExD_31_0x80000000 = 2147483648, // 0x80000000
    }
}