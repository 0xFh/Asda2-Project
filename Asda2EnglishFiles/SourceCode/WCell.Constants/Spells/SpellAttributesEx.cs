using System;

namespace WCell.Constants.Spells
{
    [Flags]
    public enum SpellAttributesEx : uint
    {
        None = 0,
        AttrEx_0_0x1 = 1,
        DrainEntireManaPool = 2,
        Channeled_1 = 4,
        AttrEx_3_0x8 = 8,
        AttrEx_4_0x10 = 16, // 0x00000010
        RemainStealthed = 32, // 0x00000020
        Channeled_2 = 64, // 0x00000040
        Negative = 128, // 0x00000080
        TargetNotInCombat = 256, // 0x00000100
        AttrEx_9_0x200 = 512, // 0x00000200
        AttrEx_10_0x400 = 1024, // 0x00000400
        AttrEx_11_0x800 = 2048, // 0x00000800
        PickPocket = 4096, // 0x00001000
        ChangeSight = 8192, // 0x00002000
        AttrEx_14_0x4000 = 16384, // 0x00004000
        DispelAurasOnImmunity = 32768, // 0x00008000
        UnaffectedBySchoolImmunity = 65536, // 0x00010000
        RemainOutOfCombat = 131072, // 0x00020000
        AttrEx_18_0x40000 = 262144, // 0x00040000
        CannotTargetSelf = 524288, // 0x00080000
        MustBeBehindTarget = 1048576, // 0x00100000
        AttrEx_21_0x200000 = 2097152, // 0x00200000
        FinishingMove = 4194304, // 0x00400000
        AttrEx_23_0x800000 = 8388608, // 0x00800000
        AttrEx_24_0x1000000 = 16777216, // 0x01000000
        AttrEx_25_0x2000000 = 33554432, // 0x02000000
        AttrEx_26_0x4000000 = 67108864, // 0x04000000
        AttrEx_27_0x8000000 = 134217728, // 0x08000000
        AttrEx_28_0x10000000 = 268435456, // 0x10000000
        AttrEx_29_0x20000000 = 536870912, // 0x20000000
        Overpower = 1073741824, // 0x40000000
        AttrEx_31_0x80000000 = 2147483648, // 0x80000000
    }
}