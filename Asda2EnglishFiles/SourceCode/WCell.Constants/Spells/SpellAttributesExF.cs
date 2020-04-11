using System;

namespace WCell.Constants.Spells
{
    [Flags]
    public enum SpellAttributesExF : uint
    {
        None = 0,
        AttrExF_0_0x1 = 1,
        AttrExF_1_0x2 = 2,
        AttrExF_2_0x4 = 4,
        AttrExF_3_0x8 = 8,
        AttrExF_4_0x10 = 16, // 0x00000010
        AttrExF_5_0x20 = 32, // 0x00000020
        AttrExF_6_0x40 = 64, // 0x00000040
        AttrExF_7_0x80 = 128, // 0x00000080
        AttrExF_8_0x100 = 256, // 0x00000100
        AttrExF_9_0x200 = 512, // 0x00000200
        AttrExF_10_0x400 = 1024, // 0x00000400
        AttrExF_11_0x800 = 2048, // 0x00000800
        AttrExF_12_0x1000 = 4096, // 0x00001000
        AttrExF_13_0x2000 = 8192, // 0x00002000
        AttrExF_14_0x4000 = 16384, // 0x00004000
        AttrExF_15_0x8000 = 32768, // 0x00008000
        AttrExF_16_0x10000 = 65536, // 0x00010000
        AttrExF_17_0x20000 = 131072, // 0x00020000
        AttrExF_18_0x40000 = 262144, // 0x00040000
        AttrExF_19_0x80000 = 524288, // 0x00080000
        AttrExF_20_0x100000 = 1048576, // 0x00100000
        AttrExF_21_0x200000 = 2097152, // 0x00200000
        AttrExF_22_0x400000 = 4194304, // 0x00400000
        AttrExF_23_0x800000 = 8388608, // 0x00800000
        AttrExF_24_0x1000000 = 16777216, // 0x01000000
        AttrExF_25_0x2000000 = 33554432, // 0x02000000
        AttrExF_26_0x4000000 = 67108864, // 0x04000000
        AttrExF_27_0x8000000 = 134217728, // 0x08000000
        AttrExF_28_0x10000000 = 268435456, // 0x10000000
        AttrExF_29_0x20000000 = 536870912, // 0x20000000
        AttrExF_30_0x40000000 = 1073741824, // 0x40000000
        AttrExF_31_0x80000000 = 2147483648, // 0x80000000
    }
}