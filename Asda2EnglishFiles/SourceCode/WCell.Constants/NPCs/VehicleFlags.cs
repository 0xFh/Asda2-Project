using System;

namespace WCell.Constants.NPCs
{
    [Flags]
    [Serializable]
    public enum VehicleFlags
    {
        PreventStrafe = 1,
        PreventJumping = 2,
        FullSpeedTurning = 4,
        Flagx8 = 8,
        AlwaysAllowPitching = 16, // 0x00000010
        FullSpeedPitching = 32, // 0x00000020
        CustomPitch = 64, // 0x00000040
        Flagx80 = 128, // 0x00000080
        Flagx100 = 256, // 0x00000100
        Flagx200 = Flagx80 | CustomPitch | Flagx8, // 0x000000C8
        AimAngleAdjustable = 1024, // 0x00000400
        AimPowerAdjustable = 2048, // 0x00000800
    }
}