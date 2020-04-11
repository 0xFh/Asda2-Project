using System;

namespace WCell.Constants.NPCs
{
    [Flags]
    [Serializable]
    public enum VehicleSeatFlagsB : uint
    {
        None = 0,
        Flagx1 = 1,
        UsableForced = 2,
        Flagx4 = 4,
        TargetsInRaidUI = 8,
        Flagx10 = 16, // 0x00000010
        Ejectable = 32, // 0x00000020
        UsableForced2 = 64, // 0x00000040
        Flagx80 = 128, // 0x00000080
        UsableForced3 = 256, // 0x00000100
        CanSwitchSeats = 67108864, // 0x04000000
        VehiclePlayerFrameUI = 2147483648, // 0x80000000
    }
}