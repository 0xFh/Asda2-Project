using System;

namespace WCell.Constants
{
    /// <summary>Group Type</summary>
    [Flags]
    public enum GroupFlags : byte
    {
        Party = 0,
        Raid = 1,
        Battleground = 2,
        BattlegroundOrRaid = Battleground | Raid, // 0x03
        LFD = 8,
    }
}