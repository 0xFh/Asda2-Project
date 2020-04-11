using System;

namespace WCell.Constants.Factions
{
    [Flags]
    public enum FactionFlags : byte
    {
        None = 0,
        Visible = 1,
        AtWar = 2,
        Hidden = 4,
        Inivisible = 8,
        Peace = 16, // 0x10
        Inactive = 32, // 0x20
        Rival = 64, // 0x40
    }
}