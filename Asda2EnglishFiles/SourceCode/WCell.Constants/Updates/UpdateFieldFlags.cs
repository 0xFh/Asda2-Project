using System;

namespace WCell.Constants.Updates
{
    [Flags]
    public enum UpdateFieldFlags : uint
    {
        None = 0,
        Public = 1,
        Private = 2,
        OwnerOnly = 4,
        Flag_0x8_Unused = 8,
        ItemOwner = 16, // 0x00000010
        BeastLore = 32, // 0x00000020
        GroupOnly = 64, // 0x00000040
        Flag_0x80_Unused = 128, // 0x00000080
        Dynamic = 256, // 0x00000100
    }
}