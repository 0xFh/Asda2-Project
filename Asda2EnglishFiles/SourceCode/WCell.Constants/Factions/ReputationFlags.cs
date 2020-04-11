using System;

namespace WCell.Constants.Factions
{
    [Flags]
    public enum ReputationFlags
    {
        None = 0,
        Visible = 1,
        AtWar = 2,
        Hidden = 4,
        ForcedInvisible = 8,
        ForcedPeace = 16, // 0x00000010
        Inactive = 32, // 0x00000020
        Flag_0x40 = 64, // 0x00000040
        Expansion_2 = 128, // 0x00000080
    }
}