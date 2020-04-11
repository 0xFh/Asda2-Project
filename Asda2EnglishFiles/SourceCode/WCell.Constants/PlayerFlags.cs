using System;

namespace WCell.Constants
{
    [Flags]
    public enum PlayerFlags : uint
    {
        None = 0,
        GroupLeader = 1,
        AFK = 2,
        DND = 4,
        GM = 8,
        Ghost = 16, // 0x00000010
        Resting = 32, // 0x00000020
        Flag_0x40 = 64, // 0x00000040
        FreeForAllPVP = 128, // 0x00000080
        InPvPSanctuary = 256, // 0x00000100
        PVP = 512, // 0x00000200
        HideHelm = 1024, // 0x00000400
        HideCloak = 2048, // 0x00000800
        PartialPlayTime = 4096, // 0x00001000
        NoPlayTime = 8192, // 0x00002000
        OutOfBounds = 16384, // 0x00004000
        Developer = 32768, // 0x00008000
        AllowLowLevelRaid = 65536, // 0x00010000
        Flag_0x20000 = 131072, // 0x00020000
        PVPTimerActive = 262144, // 0x00040000
    }
}