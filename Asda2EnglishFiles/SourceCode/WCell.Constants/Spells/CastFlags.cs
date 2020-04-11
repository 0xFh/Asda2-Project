using System;

namespace WCell.Constants.Spells
{
    [Flags]
    public enum CastFlags : uint
    {
        None = 0,
        Flag_0x1 = 1,
        Flag_0x2 = 2,
        Flag_0x4 = 4,
        Flag_0x8 = 8,
        Flag_0x10 = 16, // 0x00000010
        Ranged = 32, // 0x00000020
        Flag_0x40 = 64, // 0x00000040
        Flag_0x80 = 128, // 0x00000080
        Flag_0x100 = 256, // 0x00000100
        Flag_0x200 = 512, // 0x00000200
        Flag_0x400 = 1024, // 0x00000400
        RunicPowerGain = 2048, // 0x00000800
        Flag_0x10000 = 65536, // 0x00010000
        Flag_0x20000 = 131072, // 0x00020000
        RuneAbility = 262144, // 0x00040000
        Flag_0x80000 = 524288, // 0x00080000
        Flag_0x100000 = 1048576, // 0x00100000
        RuneCooldownList = 2097152, // 0x00200000
        Flag_0x4000000 = 67108864, // 0x04000000
    }
}