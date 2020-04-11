using System;

namespace WCell.Constants.Updates
{
    [Flags]
    public enum UpdateFlags : uint
    {
        Self = 1,
        Transport = 2,
        AttackingTarget = 4,
        Flag_0x8 = 8,
        Flag_0x10 = 16, // 0x00000010
        Living = 32, // 0x00000020
        StationaryObject = 64, // 0x00000040
        Vehicle = 128, // 0x00000080
        StationaryObjectOnTransport = 256, // 0x00000100
        HasRotation = 512, // 0x00000200
    }
}