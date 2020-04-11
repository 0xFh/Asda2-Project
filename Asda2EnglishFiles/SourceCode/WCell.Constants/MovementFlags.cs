using System;

namespace WCell.Constants
{
    [Flags]
    public enum MovementFlags : uint
    {
        None = 0,
        Forward = 1,
        Backward = 2,
        StrafeLeft = 4,
        StrafeRight = 8,
        TurnLeft = 16, // 0x00000010
        TurnRight = 32, // 0x00000020
        PitchUp = 64, // 0x00000040
        PitchDown = 128, // 0x00000080
        WalkMode = 256, // 0x00000100
        OnTransport = 512, // 0x00000200
        DisableGravity = 1024, // 0x00000400
        Root = 2048, // 0x00000800
        Falling = 4096, // 0x00001000
        FallingFar = 8192, // 0x00002000
        PendingStop = 16384, // 0x00004000
        PendingStrafeStop = 32768, // 0x00008000
        PendingForward = 65536, // 0x00010000
        PendingBackward = 131072, // 0x00020000
        PendingStrafeLeft = 262144, // 0x00040000
        PendingStrafeRight = 524288, // 0x00080000
        PendingRoot = 1048576, // 0x00100000
        Swimming = 2097152, // 0x00200000
        Ascending = 4194304, // 0x00400000
        Descending = 8388608, // 0x00800000
        CanFly = 16777216, // 0x01000000
        Flying = 33554432, // 0x02000000
        SplineElevation = 67108864, // 0x04000000
        SplineEnabled = 134217728, // 0x08000000
        Waterwalking = 268435456, // 0x10000000
        CanSafeFall = 536870912, // 0x20000000
        Hover = 1073741824, // 0x40000000
        LocalDirty = 2147483648, // 0x80000000

        MaskDirections =
            PitchDown | PitchUp | TurnRight | TurnLeft | StrafeRight | StrafeLeft | Backward | Forward, // 0x000000FF

        MaskMoving =
            Descending | Ascending | FallingFar | Falling | TurnRight | TurnLeft | StrafeRight | StrafeLeft | Backward |
            Forward, // 0x00C0303F
        AwaitingLoad = SplineEnabled, // 0x08000000
    }
}