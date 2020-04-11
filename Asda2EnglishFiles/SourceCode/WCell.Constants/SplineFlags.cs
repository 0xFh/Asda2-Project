using System;

namespace WCell.Constants
{
    [Flags]
    public enum SplineFlags : uint
    {
        None = 0,
        Flag_0x8 = 8,
        SplineSafeFall = 16, // 0x00000010
        Flag_0x20 = 32, // 0x00000020
        Flag_0x40 = 64, // 0x00000040
        Flag_0x80 = 128, // 0x00000080
        Done = 256, // 0x00000100
        Falling = 512, // 0x00000200
        NotSplineMover = 1024, // 0x00000400
        Parabolic = 2048, // 0x00000800
        Walkmode = 4096, // 0x00001000
        Flying = 8192, // 0x00002000
        Knockback = 16384, // 0x00004000
        FinalFacePoint = 32768, // 0x00008000
        FinalFaceTarget = 65536, // 0x00010000
        FinalFaceAngle = 131072, // 0x00020000
        CatmullRom = 262144, // 0x00040000
        Cyclic = 524288, // 0x00080000
        EnterCycle = 1048576, // 0x00100000
        AnimationTier = 2097152, // 0x00200000
        Frozen = 4194304, // 0x00400000
        Unknown5 = 8388608, // 0x00800000
        Unknown6 = 16777216, // 0x01000000
        Unknown7 = 33554432, // 0x02000000
        Unknown8 = 67108864, // 0x04000000
        Backward = 134217728, // 0x08000000
        UsePathSmoothing = 268435456, // 0x10000000
        SplineCanSwim = 536870912, // 0x20000000
        UncompressedPath = 1073741824, // 0x40000000
        Unknown13 = 2147483648, // 0x80000000
    }
}