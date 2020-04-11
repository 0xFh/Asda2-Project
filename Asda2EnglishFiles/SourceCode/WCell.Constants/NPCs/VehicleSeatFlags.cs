using System;

namespace WCell.Constants.NPCs
{
    [Flags]
    [Serializable]
    public enum VehicleSeatFlags : uint
    {
        None = 0,
        HasLowerAnimForEnter = 1,
        HasLowerAnimForRide = 2,
        Flagx4 = 4,
        ShouldUseVehicleSeatExitAnimationOnVoluntaryExit = 8,
        Flagx10 = 16, // 0x00000010
        Flagx20 = 32, // 0x00000020
        Flagx40 = 64, // 0x00000040
        Flagx80 = 128, // 0x00000080
        Flagx100 = 256, // 0x00000100
        HidePassenger = 512, // 0x00000200
        Flagx400 = 1024, // 0x00000400
        VehicleControlSeat = 2048, // 0x00000800
        Flagx1000 = 4096, // 0x00001000
        Uncontrolled = 8192, // 0x00002000
        CanAttack = 16384, // 0x00004000
        ShouldUseVehicleSeatExitAnimationOnForcedExit = 32768, // 0x00008000
        Flagx10000 = 65536, // 0x00010000
        Flagx20000 = 131072, // 0x00020000
        HasVehicleExitAnimForVoluntaryExit = 262144, // 0x00040000
        HasVehicleExitAnimForForcedExit = 524288, // 0x00080000
        Flagx100000 = 1048576, // 0x00100000
        Flagx200000 = 2097152, // 0x00200000
        RecHasVehicleEnterAnim = 4194304, // 0x00400000
        Flagx800000 = 8388608, // 0x00800000
        EnableVehicleZoom = 16777216, // 0x01000000
        CanEnterorExit = 33554432, // 0x02000000
        CanSwitchSeats = 67108864, // 0x04000000
        HasStartWaitingForVehicleTransitionAnim_Enter = 134217728, // 0x08000000
        HasStartWaitingForVehicleTransitionAnim_Exit = 268435456, // 0x10000000
        CanCast = 536870912, // 0x20000000
        Flagx40000000 = 1073741824, // 0x40000000
        AllowsInteraction = 2147483648, // 0x80000000
    }
}