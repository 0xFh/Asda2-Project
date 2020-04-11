using System;

namespace WCell.Constants
{
    [Flags]
    public enum UnitFlags : uint
    {
        None = 0,
        Flag_0_0x1 = 1,
        SelectableNotAttackable = 2,
        Influenced = 4,
        PlayerControlled = 8,
        Flag_0x10 = 16, // 0x00000010
        Preparation = 32, // 0x00000020
        PlusMob = 64, // 0x00000040
        SelectableNotAttackable_2 = 128, // 0x00000080
        NotAttackable = 256, // 0x00000100
        Passive = 512, // 0x00000200
        Looting = 1024, // 0x00000400
        PetInCombat = 2048, // 0x00000800
        Flag_12_0x1000 = 4096, // 0x00001000
        Silenced = 8192, // 0x00002000
        Flag_14_0x4000 = 16384, // 0x00004000
        Flag_15_0x8000 = 32768, // 0x00008000
        SelectableNotAttackable_3 = 65536, // 0x00010000
        Pacified = 131072, // 0x00020000
        Stunned = 262144, // 0x00040000
        CanPerformAction_Mask1 = Stunned | Pacified, // 0x00060000
        Combat = 524288, // 0x00080000
        TaxiFlight = 1048576, // 0x00100000
        Disarmed = 2097152, // 0x00200000
        Confused = 4194304, // 0x00400000
        Feared = 8388608, // 0x00800000
        Possessed = 16777216, // 0x01000000
        NotSelectable = 33554432, // 0x02000000
        Skinnable = 67108864, // 0x04000000
        Mounted = 134217728, // 0x08000000
        Flag_28_0x10000000 = 268435456, // 0x10000000
        Flag_29_0x20000000 = 536870912, // 0x20000000
        Flag_30_0x40000000 = 1073741824, // 0x40000000
        Flag_31_0x80000000 = 2147483648, // 0x80000000
    }
}