using System;

namespace WCell.Constants
{
    [Flags]
    public enum UnitFlags2
    {
        FeignDeath = 1,
        NoModel = 2,
        Flag_0x4 = 4,
        Flag_0x8 = 8,
        MirrorImage = 16, // 0x00000010
        Flag_0x20 = 32, // 0x00000020
        ForceAutoRunForward = 64, // 0x00000040
        Flag_0x80 = 128, // 0x00000080
        Flag_0x400 = 1024, // 0x00000400
        RegeneratePower = 2048, // 0x00000800
        Flag_0x1000 = 4096, // 0x00001000
    }
}