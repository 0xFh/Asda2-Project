using System;

namespace WCell.Constants.Spells
{
    [Flags]
    public enum SpellAttributesExC : uint
    {
        None = 0,
        AttrExC_0_0x1 = 1,
        AttrExC_1_0x2 = 2,
        AttrExC_2_0x4 = 4,
        AttrExC_3_0x8 = 8,
        Rebirth = 16, // 0x00000010
        AttrExC_5_0x20 = 32, // 0x00000020
        AttrExC_6_0x40 = 64, // 0x00000040
        AttrExC_7_0x80 = 128, // 0x00000080
        AttrExC_8_0x100 = 256, // 0x00000100
        AttrExC_9_0x200 = 512, // 0x00000200
        RequiresMainHandWeapon = 1024, // 0x00000400
        BattleGroundOnly = 2048, // 0x00000800
        AttrExC_12_0x1000 = 4096, // 0x00001000
        AttrExC_13_0x2000 = 8192, // 0x00002000
        HonorlessTarget = 16384, // 0x00004000
        ShootRangedWeapon = 32768, // 0x00008000
        AttrExC_16_0x10000 = 65536, // 0x00010000
        NoInitialAggro = 131072, // 0x00020000
        AttrExC_18_0x40000 = 262144, // 0x00040000
        AttrExC_19_0x80000 = 524288, // 0x00080000
        PersistsThroughDeath = 1048576, // 0x00100000
        NaturesGrasp = 2097152, // 0x00200000
        RequiresWand = 4194304, // 0x00400000
        AttrExC_23_0x800000 = 8388608, // 0x00800000
        RequiresOffHandWeapon = 16777216, // 0x01000000
        AttrExC_25_0x2000000 = 33554432, // 0x02000000
        OldOnlyInOutlands = 67108864, // 0x04000000
        AttrExC_27_0x8000000 = 134217728, // 0x08000000
        AttrExC_28_0x10000000 = 268435456, // 0x10000000
        AttrExC_29_0x20000000 = 536870912, // 0x20000000
        AttrExC_30_0x40000000 = 1073741824, // 0x40000000
        AttrExC_31_0x80000000 = 2147483648, // 0x80000000
        RequiresTwoWeapons = RequiresOffHandWeapon | RequiresMainHandWeapon, // 0x01000400
    }
}