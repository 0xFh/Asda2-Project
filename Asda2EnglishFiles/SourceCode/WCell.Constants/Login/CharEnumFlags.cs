using System;

namespace WCell.Constants.Login
{
    [Flags]
    public enum CharEnumFlags
    {
        None = 0,
        Alive = 1,
        LockedForTransfer = 4,
        HideHelm = 1024, // 0x00000400
        HideCloak = 2048, // 0x00000800
        Ghost = 8192, // 0x00002000
        NeedsRename = 16384, // 0x00004000
        Unknown = 10485760, // 0x00A00000
        LockedForBilling = 16777216, // 0x01000000
    }
}