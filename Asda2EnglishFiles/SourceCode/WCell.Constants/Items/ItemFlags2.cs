using System;

namespace WCell.Constants.Items
{
    [Flags]
    public enum ItemFlags2 : uint
    {
        HordeOnly = 1,
        AllianceOnly = 2,
        ExtendedCostRequiresGold = 4,
        Unknown4 = 8,
        Unknown5 = 16, // 0x00000010
        Unknown6 = 32, // 0x00000020
        Unknown7 = 64, // 0x00000040
        Unknown8 = 128, // 0x00000080
        NeedRollDisabled = 256, // 0x00000100
        CasterWeapon = 512, // 0x00000200
    }
}