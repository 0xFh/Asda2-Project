using System;

namespace WCell.Constants
{
    [Flags]
    public enum MovementFlags2 : ushort
    {
        None = 0,
        PreventStrafe = 1,
        PreventJumping = 2,
        DisableCollision = 4,
        FullSpeedTurning = 8,
        FullSpeedPitching = 16, // 0x0010
        AlwaysAllowPitching = 32, // 0x0020
        IsVehicleExitVoluntary = 64, // 0x0040
        IsJumpSplineInAir = 128, // 0x0080
        IsAnimTierInTrans = 256, // 0x0100
        PreventChangePitch = 512, // 0x0200
        InterpolateMove = 1024, // 0x0400
        InterpolateTurning = 2048, // 0x0800
        InterpolatePitching = 4096, // 0x1000
        VehiclePassengerIsTransitionAllowed = 8192, // 0x2000
        CanTransitionBetweenSwimAndFly = 16384, // 0x4000
        Flag_0x8000 = 32768, // 0x8000
        Status_400 = InterpolateMove, // 0x0400
        Status_800 = InterpolateTurning, // 0x0800
        InterpMask = Status_800 | Status_400 | InterpolatePitching, // 0x1C00
    }
}