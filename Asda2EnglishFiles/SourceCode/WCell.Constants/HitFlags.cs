using System;

namespace WCell.Constants
{
    [Flags]
    public enum HitFlags : uint
    {
        NormalSwing = 0,
        HitFlag_0x1 = 1,
        PlayWoundAnimation = 2,
        OffHand = 4,
        HitFlag_0x8 = 8,
        Miss = 16, // 0x00000010
        AbsorbType1 = 32, // 0x00000020
        AbsorbType2 = 64, // 0x00000040
        ResistType1 = 128, // 0x00000080
        ResistType2 = 256, // 0x00000100
        CriticalStrike = 512, // 0x00000200
        HitFlag_0x400 = 1024, // 0x00000400
        HitFlag_0x800 = 2048, // 0x00000800
        HitFlag_0x1000 = 4096, // 0x00001000
        Block = 8192, // 0x00002000
        HideWorldTextForNoDamage = 16384, // 0x00004000
        BloodSpurtInBack = 32768, // 0x00008000
        Glancing = 65536, // 0x00010000
        Crushing = 131072, // 0x00020000
        Ignore = 262144, // 0x00040000
        SwingNoHitSound = 524288, // 0x00080000
        HitFlag_0x100000 = 1048576, // 0x00100000
        HitFlag_0x200000 = 2097152, // 0x00200000
        HitFlag_0x400000 = 4194304, // 0x00400000
        ModifyPredictedPower = 8388608, // 0x00800000
        ForceShowBloodSpurt = 16777216, // 0x01000000
        HitFlag_0x2000000 = 33554432, // 0x02000000
        HitFlag_0x4000000 = 67108864, // 0x04000000
        HitFlag_0x8000000 = 134217728, // 0x08000000
    }
}